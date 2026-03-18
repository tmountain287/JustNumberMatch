using Firebase;
using Firebase.Firestore;   // Firestore 사용 (예외 타입 이름 직접 참조는 안 함)
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum SaveResultType
{
    Success,
    PermissionDenied,
    NetworkOrTimeout,
    UnknownError
}

public sealed class SaveResult
{
    public SaveResultType Type;
    public string ConflictDeviceId;
    public string ConflictRecord;
    public DateTime? ConflictUpdatedAt;
    public string Message;
}

public class FirestoreDiag : MonoSingletonDont<FirestoreDiag>
{
    private FirebaseFirestore db;
    private readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(10);
    private readonly int maxRetries = 1; // 총 2회(초기 1 + 재시도 1)

    public void Initialize()
    {
        db = FirebaseFirestore.DefaultInstance;
        db.EnableNetworkAsync();
    }

    public async void InitSync()
    {
        // 1) Firebase SDK 의존성 확인
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep == DependencyStatus.Available)
        {
            // 2) Firestore 인스턴스 가져오기
            db = FirebaseFirestore.DefaultInstance;

            Debug.Log("[Firebase] Firestore 초기화 완료");
            // FirestoreUserDataClient에 주입

            await db.EnableNetworkAsync();
            // 예: 저장 호출          
        }
        else
        {
            Debug.LogError($"[Firebase] Firebase not available: {dep}");
        }
    }

    // ----------------- Public APIs -----------------
    /// 기본 저장: 동일 device_id면 통과. 권한 거절 시 Conflict 정보 반환.
    public async Task<SaveResult> TrySaveAsync(
    string us_id,
    string device_id,
    string record,
    bool checkDevice,                 // ✅ 추가
    bool forceConflict,
    CancellationToken externalCt = default)
    {
        if (string.IsNullOrEmpty(us_id))
            return new SaveResult { Type = SaveResultType.UnknownError, Message = "us_id empty" };

        var doc = db.Collection("userdata").Document(us_id);

        var data = new Dictionary<string, object> {
        { "us_id", us_id },
        { "device_id", device_id },
        { "record", record },
        { "updatedAt", FieldValue.ServerTimestamp },
        { "forceConflict", forceConflict },
        { "checkDevice", checkDevice },
        { "isOverwrite", FieldValue.Delete },  // ApplyOverwriteAsync 잔여 플래그 제거
    };

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            cts.CancelAfter(defaultTimeout);

            try
            {
                var snap = await doc.GetSnapshotAsync();

                if (!snap.Exists)
                {
                    // 첫 저장: create 경로
                    await doc.SetAsync(data).TimeoutAfter(defaultTimeout, cts.Token);
                }
                else
                {
                    // 이후 저장: merge
                    await doc.SetAsync(data, SetOptions.MergeAll).TimeoutAfter(defaultTimeout, cts.Token);
                }

                return new SaveResult { Type = SaveResultType.Success };
            }
            catch (Exception e) when (IsPermissionDenied(e))
            {
                // ✅ 이제 device mismatch뿐 아니라 (룰에 따라) record mismatch 등도 여기로 들어옴
                return await BuildConflictResultAsync(doc, cts.Token, "Permission denied (conflict)");
            }
            catch (TimeoutException)
            {
                if (attempt == maxRetries)
                    return new SaveResult { Type = SaveResultType.NetworkOrTimeout, Message = "Timeout" };
                await Task.Delay(BackoffMs(attempt), externalCt);
            }
            catch (TaskCanceledException)
            {
                if (attempt == maxRetries)
                    return new SaveResult { Type = SaveResultType.NetworkOrTimeout, Message = "Canceled/Timeout" };
                await Task.Delay(BackoffMs(attempt), externalCt);
            }
            catch (Exception e)
            {
                if (attempt == maxRetries)
                    return new SaveResult { Type = SaveResultType.UnknownError, Message = e.Message };
                await Task.Delay(BackoffMs(attempt), externalCt);
            }
        }

        return new SaveResult { Type = SaveResultType.UnknownError, Message = "Unreachable" };
    }

    /// Intent 단계: 규칙 ② 경로. device_id 바꾸지 말고 overwriteIntent만 세팅.
    public async Task<bool> CreateOverwriteIntentAsync(string us_id, string requestDeviceId, TimeSpan? ttl = null, CancellationToken externalCt = default)
    {
        var doc = db.Collection("userdata").Document(us_id);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        cts.CancelAfter(defaultTimeout);

        try
        {
            var snap = await doc.GetSnapshotAsync().TimeoutAfter(defaultTimeout, cts.Token);
            if (!snap.Exists) return false;

            if (!snap.TryGetValue<string>("device_id", out var currentDeviceId))
                return false;

            var intent = new Dictionary<string, object> {
                { "prev_device_id", currentDeviceId },
                { "requestDeviceId", requestDeviceId },
                { "expiresAt", Timestamp.FromDateTime(DateTime.UtcNow.Add(ttl ?? TimeSpan.FromMinutes(2))) }
            };

            var data = new Dictionary<string, object> {
                { "us_id", us_id },
                { "device_id", currentDeviceId },     // 변동 금지
                { "overwriteIntent", intent },
                { "updatedAt", FieldValue.ServerTimestamp },
                // { "record", ... } // 필요 시 포함(규칙 hasOnly 허용 범위)
            };

            await doc.SetAsync(data, SetOptions.MergeAll).TimeoutAfter(defaultTimeout, cts.Token);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firestore] Intent ERROR: {e.Message}");
            return false;
        }
    }

    /// 실제 덮어쓰기: 규칙 ③ 경로. device_id 변경 + 같은 저장에서 overwriteIntent 삭제.
    public async Task<bool> ApplyOverwriteAsync(string us_id, string newDeviceId, string newRecord, CancellationToken externalCt = default)
    {
        var doc = db.Collection("userdata").Document(us_id);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        cts.CancelAfter(defaultTimeout);

        try
        {
            await db.RunTransactionAsync(async tx =>
            {
                var snap = await tx.GetSnapshotAsync(doc);
                if (!snap.Exists) throw new Exception("userdata not found");

                var data = new Dictionary<string, object> {
                    { "us_id", us_id },
                    { "device_id", newDeviceId },
                    { "record", newRecord },
                    { "updatedAt", FieldValue.ServerTimestamp },
                    { "isOverwrite", true },  // Firestore 규칙: 덮어쓰기 경로 허용
                    { "overwriteIntent", FieldValue.Delete } // 같은 저장에서 삭제 필수
                };

                tx.Set(doc, data, SetOptions.MergeAll);
            }).TimeoutAfter(defaultTimeout, cts.Token);

            return true;
        }
        catch (Exception e) when (IsPermissionDenied(e))
        {
            Debug.LogWarning("[Firestore] Overwrite denied. Intent missing/expired or device mismatch.");
            return false;
        }
        catch (TimeoutException)
        {
            Debug.LogError("[Firestore] Overwrite timeout.");
            return false;
        }
        catch (TaskCanceledException)
        {
            Debug.LogError("[Firestore] Overwrite canceled/timeout.");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firestore] Overwrite ERROR: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// userdata 문서 삭제. 보안 규칙: authedOwner() 일 때만 허용 (로그인한 사용자 본인만).
    /// </summary>
    public async Task<bool> DeleteUserDataAsync(string us_id, CancellationToken externalCt = default)
    {
        if (string.IsNullOrEmpty(us_id))
            return false;

        var doc = db.Collection("userdata").Document(us_id);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        cts.CancelAfter(defaultTimeout);

        try
        {
            await doc.DeleteAsync().TimeoutAfter(defaultTimeout, cts.Token);
            Debug.Log("[Firestore] userdata 문서 삭제 완료.");
            return true;
        }
        catch (Exception e) when (IsPermissionDenied(e))
        {
            Debug.LogWarning("[Firestore] Delete denied (not owner or not authenticated).");
            return false;
        }
        catch (TimeoutException)
        {
            Debug.LogError("[Firestore] Delete timeout.");
            return false;
        }
        catch (TaskCanceledException)
        {
            Debug.LogError("[Firestore] Delete canceled/timeout.");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Firestore] Delete ERROR: {e.Message}");
            return false;
        }
    }

    /// 저장 시도 후 Conflict 필요 시 이 메서드 호출 결과로 UI 분기
    //public Task<SaveResult> SaveOrGetConflictAsync(string us_id, string device_id, string record, CancellationToken ct = default)
    //    => TrySaveAsync(us_id, device_id, record, ct);

    // ----------------- Helpers -----------------

    private async Task<SaveResult> BuildConflictResultAsync(DocumentReference doc, CancellationToken ct, string msg)
    {
        try
        {
            var snap = await doc.GetSnapshotAsync().TimeoutAfter(defaultTimeout, ct);
            if (!snap.Exists)
            {
                return new SaveResult
                {
                    Type = SaveResultType.PermissionDenied,
                    Message = "Permission denied & doc not found (race?)"
                };
            }

            snap.TryGetValue<string>("device_id", out var cloudDev);
            string cloudRec = null;

            if (snap.TryGetValue<string>("record", out var recStr))
                cloudRec = recStr;
            else if (snap.TryGetValue<object>("record", out var any))
                cloudRec = any?.ToString();

            DateTime? updatedAt = null;
            if (snap.TryGetValue<Timestamp>("updatedAt", out var ts))
                updatedAt = ts.ToDateTime();

            return new SaveResult
            {
                Type = SaveResultType.PermissionDenied,
                ConflictDeviceId = cloudDev,
                ConflictRecord = cloudRec,
                ConflictUpdatedAt = updatedAt,
                Message = msg
            };
        }
        catch (Exception readErr)
        {
            return new SaveResult
            {
                Type = SaveResultType.PermissionDenied,
                Message = "Permission denied & read fail: " + readErr.Message
            };
        }
    }

    private static int BackoffMs(int attempt) => (int)(500 * Math.Pow(2, attempt));

    /// SDK/버전과 무관하게 PermissionDenied를 판별하는 유틸
    private static bool IsPermissionDenied(Exception ex)
    {
        // 예외 체인 전체를 검사
        for (var e = ex; e != null; e = e.InnerException)
        {
            try
            {
                var msg = e.ToString(); // 타입+메시지 포함
                if (msg.IndexOf("PERMISSION_DENIED", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (msg.IndexOf("permission-denied", StringComparison.OrdinalIgnoreCase) >= 0) return true; // Web SDK 스타일
                if (msg.IndexOf("StatusCode.PermissionDenied", StringComparison.OrdinalIgnoreCase) >= 0) return true; // gRPC 스타일

                // 리플렉션으로 ErrorCode 같은 프로퍼티가 있으면 문자열로 판단
                var prop = e.GetType().GetProperty("ErrorCode");
                if (prop != null)
                {
                    var val = prop.GetValue(e, null)?.ToString();
                    if (!string.IsNullOrEmpty(val) &&
                        val.IndexOf("PermissionDenied", StringComparison.OrdinalIgnoreCase) >= 0)
                        return true;
                }
            }
            catch { /* ignore */ }
        }
        return false;
    }
}

#region Task Timeout Extensions
public static class TaskTimeoutExtensions
{
    public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var delay = Task.Delay(timeout, cts.Token);
        var completed = await Task.WhenAny(task, delay);
        if (completed == delay) throw new TimeoutException($"Task timed out after {timeout.TotalSeconds:0.#}s");
        cts.Cancel();
        return await task; // 예외 전파/결과 수거
    }

    public static async Task TimeoutAfter(this Task task, TimeSpan timeout, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var delay = Task.Delay(timeout, cts.Token);
        var completed = await Task.WhenAny(task, delay);
        if (completed == delay) throw new TimeoutException($"Task timed out after {timeout.TotalSeconds:0.#}s");
        cts.Cancel();
        await task; // 예외 전파
    }
}
#endregion

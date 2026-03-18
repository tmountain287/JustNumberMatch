using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseFirestoreManager : MonoSingletonDont<FirebaseFirestoreManager>
{
    private FirebaseFirestore db;

    public void Initialize()
    {
        db = FirebaseFirestore.DefaultInstance;
    }

    public async Task<string> SaveData(string us_id, string record)
    {
        Debug.Log(record);
        try
        {
            var data = new Dictionary<string, object> {
            { "us_id", us_id },
            { "record", record },
        };

            Debug.Log(us_id);
            await db.Collection("users").Document(us_id).SetAsync(data);
            Debug.Log("Firestore 저장 완료 (최신값)");
            return "Success";
        }
        catch (Exception e)
        {
            if (e is Firebase.FirebaseException fe)
                Debug.LogError($"Firestore 오류: {fe.ErrorCode} / {fe.Message}");
            else
                Debug.LogError(e);

            return "Error";
        }
    }

    /// <summary>
    /// 특정 us_id의 record 값 읽어오기
    /// </summary>
    public async Task<string> GetRecord(string us_id)
    {
        DocumentSnapshot snapshot = await db.Collection("users").Document(us_id).GetSnapshotAsync();

        if (snapshot.Exists && snapshot.ContainsField("record"))
        {
            string record = snapshot.GetValue<string>("record");
            Debug.Log($"Firestore 읽기 완료: us_id={us_id}, record={record}");
            return record;
        }
        else
        {
            Debug.LogWarning($"해당 us_id({us_id}) 문서가 존재하지 않거나 record 없음");
            return null;
        }
    }
}

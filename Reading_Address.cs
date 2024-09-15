using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using Unity.VisualScripting;
using System.Data.Common;
using System.Linq;
using System;
using UnityEngine.Events;
using TMPro;
using UnityEditor.PackageManager.Requests;
using JetBrains.Annotations;

public class Get_address : MonoBehaviour
{
    
    public RootAddress addressInfo;
    public RootUTXO utxosInfo;
    
    [SerializeField]
    public double balance;
    public TMP_Text balanceText;
    public int tx_count;
    public int tx_mempool;
    public TMP_Text txCountText;

    public TMP_Text days_30Text;
    public long days_30;
    public TMP_Text days_7Text;
    public long days_7;

    private void Start()
    {
        StartCoroutine(GetAddressUTXO());
        StartCoroutine(GetAddress());
          
    }
    public IEnumerator GetAddress()
    {
        string blockHashTipUrl = "https://mempool.space/api/address/32ixEdVJWo3kmvJGMTZq5jAQVZZeuwnqzo";
        
        using (UnityWebRequest request = UnityWebRequest.Get(blockHashTipUrl))
        {

            yield return request.SendWebRequest();


            if (request.result == UnityWebRequest.Result.ConnectionError)

            {
                Debug.LogError(request.error);

            }

            else

            {
                string downloadedData = Encoding.UTF8.GetString(request.downloadHandler.data);
                addressInfo = JsonUtility.FromJson<RootAddress>(downloadedData);
                balance = addressInfo.chain_stats.funded_txo_sum / 100000000.00000000;

                tx_count = addressInfo.chain_stats.funded_txo_count;
                tx_mempool = addressInfo.mempool_stats.funded_txo_count;
                txCountText.text = "This address has " + tx_count + " confirmed Txs and " + tx_mempool + " Txs in the mempool";
            }
            
        }
    }
   

    public IEnumerator GetAddressUTXO()
    {
        string blockHashTipUrl = "https://mempool.space/api/address/32ixEdVJWo3kmvJGMTZq5jAQVZZeuwnqzo/utxo";
        
        using (UnityWebRequest request = UnityWebRequest.Get(blockHashTipUrl))
        {

            yield return request.SendWebRequest();


            if (request.result == UnityWebRequest.Result.ConnectionError)

            {
                Debug.LogError(request.error);

            }

            else

            {
                string downloadedData = Encoding.UTF8.GetString(request.downloadHandler.data);

                //Fixing Data from mempool cause the GameEngine doesn't support the format (Unity3D v 2022.3.38f1)
                downloadedData.Remove(0,1);
                downloadedData = "{\"data\": " + downloadedData + "}";

                utxosInfo = JsonUtility.FromJson<RootUTXO>(downloadedData);

                Show_SevenDays();
                Show_thirtyDays();  
            }
            
        }

    }



    public void ShowBalance()
    {
        balanceText.text = balance.ToString() + " Bitcoins en nuestra reserva";
    }

     void Show_SevenDays()
    {
        DateTime now = DateTime.Now;
        long currentTimestamp = (long)(now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        long seven_DaysAgo = currentTimestamp - 60 * 60 * 24 * 7;
        long localvalue = 0;
        foreach (Datum data in utxosInfo.data)
        {
            if(data.status.block_time >= seven_DaysAgo)
            {
                
                localvalue += data.value;
                Debug.Log(localvalue);
                days_7 = localvalue / 100000000;
            }
        }
        
    }

     void Show_thirtyDays()
    {
        DateTime now = DateTime.Now;
        long currentTimestamp = (long)(now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        long thirtyDaysAgo = currentTimestamp - 60 * 60 * 24 * 30;
        long localvalue = 0;
        foreach (Datum data in utxosInfo.data)
        {
            if(data.status.block_time >= thirtyDaysAgo)
            {
                
                localvalue += data.value;
                Debug.Log(localvalue);
                days_30 = localvalue / 100000000;
            }
        }
    }

    public void days7valueToText()
    {
        days_7Text.text = days_7 + " Bitcoins en los últimos 7 días";
    }

     public void days30valueToText()
    {
        days_30Text.text = days_30 + " Bitcoins en los últimos 30 días";
    }

}

//--> Address Info
[Serializable]
    public class ChainStats
    {
        public int funded_txo_count;
        public long funded_txo_sum;
        public int spent_txo_count;
        public int spent_txo_sum;
        public int tx_count;
    }

[Serializable]
    public class MempoolStats
    {
        public int funded_txo_count;
        public int funded_txo_sum;
        public int spent_txo_count;
        public int spent_txo_sum;
        public int tx_count;
    }

[Serializable]
    public class RootAddress
    {
        public string address;
        public ChainStats chain_stats;
        public MempoolStats mempool_stats;
    }
//---> UTXO Address info
[Serializable]
    public class Datum
    {
        public string txid;
        public int vout;
        public Status status;
        public int value;
    }
[Serializable]
    public class RootUTXO
    {
        public List<Datum> data;
    }
[Serializable]
    public class Status
    {
        public bool confirmed;
        public int block_height;
        public string block_hash;
        public int block_time;
    }

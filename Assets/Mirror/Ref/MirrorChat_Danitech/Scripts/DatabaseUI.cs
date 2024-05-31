using UnityEngine;
using UnityEngine.UI;
using MySql.Data.MySqlClient;
using System;
using System.Data;

public class DatabaseUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] InputField Input_Query;
    [SerializeField] Text Text_DBResult;
    [SerializeField] Text Text_Log;

    [Header("ConnectionInfo")]
    [SerializeField] string _ip = "127.0.0.1";
    [SerializeField] string _dbName = "test";
    [SerializeField] string _uid = "root";
    [SerializeField] string _pwd = "1234";

    private bool _isConnectTestComplete;
    private static MySqlConnection _dbConnection;


    private void Awake()
    {
        gameObject.SetActive(false);
    }
    private bool ConnectTest()
    {
        string connectStr = $"Server={_ip};Database={_dbName};Uid={_uid};Pwd={_pwd};";
        try
        {
            using (MySqlConnection conn = new MySqlConnection(connectStr))
            {
                _dbConnection = conn;
                conn.Open();
            }

            Text_Log.text = "DB 연결을 성공했습니다!";
            return true;
        }
        catch(Exception e)
        {
            Debug.LogWarning($"e:{e.ToString()}");
            Text_Log.text = "DB 연결 실패";
            return false;
        }
    }

    public void OnClick_TestDBConnect()
    {
        _isConnectTestComplete = ConnectTest();
    }

    public void SendQuery(string queryStr, string tableName)
    {
        /*if(_isConnectTestComplete == false)
        {
            Text_Log.text = "dB 연결을 먼저 시도해주세요.";
        }

        Text_Log.text = string.Empty;*/

        // 있으면 Select 관련 함수 호출
        if(queryStr.Contains("SELECT"))
        {
            DataSet dataSet = OnSelectRequest(queryStr, tableName);
            Text_DBResult.text = DeformatResult(dataSet);

        }
        else // 없다면 Insert 또는 Update 관련 쿼리
        {
            Text_DBResult.text = OnInsertOnUpdateRequest(queryStr) ? "성공" : "실패";
        }
    }

    public static bool OnInsertOnUpdateRequest(string query)
    {
        try
        {
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.Connection = _dbConnection;
            sqlCommand.CommandText = query;

            _dbConnection.Open();
            sqlCommand.ExecuteNonQuery();

            _dbConnection.Close();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    private string DeformatResult(DataSet dataSet)
    {
        string resultStr = string.Empty;

        foreach (DataTable tables in dataSet.Tables)
        {
            foreach (DataRow row in tables.Rows)
            {
                foreach(DataColumn column in tables.Columns)
                {
                    resultStr += $"{column.ColumnName} : " +
                        $"{row[column]}\n";
                }
            }
        }

        return resultStr;
    }

    public static DataSet OnSelectRequest(string query, string tableName)
        
    {
        try
        {
            _dbConnection.Open();
            MySqlCommand sqlCmd = new MySqlCommand();
            sqlCmd.Connection = _dbConnection;
            sqlCmd.CommandText = query;

            MySqlDataAdapter sd = new MySqlDataAdapter(sqlCmd);
            DataSet dataSet = new DataSet();
            sd.Fill(dataSet, tableName);

            _dbConnection.Close();
            return dataSet;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            return null;
        }
    }
    public void OnSubmit_SendQuery()
    {
        string query = string.IsNullOrWhiteSpace(Input_Query.text) ? "SELECT * FROM player_info "
            : Input_Query.text;

        SendQuery(query, "player_info");
    }

    public void OnClick_OpenDatabaseUI()
    {
        this.gameObject.SetActive(true);
    }

    public void OnClick_CloseDatabaseUI()
    {
        this.gameObject.SetActive(false);
    }

}

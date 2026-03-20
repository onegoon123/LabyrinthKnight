using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class CSVReader
{
    public static List<Dictionary<string, string>> ReadCSV(string filePath)
    {
        List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
        
        if (!File.Exists(filePath))
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {filePath}");
            return data;
        }
        
        string[] lines = File.ReadAllLines(filePath);
        
        if (lines.Length < 2)
        {
            Debug.LogError("CSV 파일에 헤더와 데이터가 없습니다.");
            return data;
        }
        
        // 헤더 파싱
        string[] headers = ParseCSVLine(lines[0]);
        
        // 데이터 파싱
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i])) continue;
            
            string[] values = ParseCSVLine(lines[i]);
            
            if (values.Length != headers.Length)
            {
                Debug.LogWarning($"행 {i + 1}의 컬럼 수가 헤더와 맞지 않습니다.");
                continue;
            }
            
            Dictionary<string, string> row = new Dictionary<string, string>();
            
            for (int j = 0; j < headers.Length; j++)
            {
                row[headers[j]] = values[j];
            }
            
            data.Add(row);
        }
        
        return data;
    }
    
    private static string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField.Trim());
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        fields.Add(currentField.Trim());
        return fields.ToArray();
    }
}

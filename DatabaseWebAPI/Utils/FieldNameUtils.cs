/*
 * Project Name:  DatabaseWebAPI
 * File Name:     FieldNameUtils.cs
 * File Function: 字段名工具函数
 * Author:        TreeHole开发组
 * Update Date:   2025-07-31
 * License:       Creative Commons Attribution 4.0 International License
 */

namespace DatabaseWebAPI.Utils;

public static class FieldNameUtils
{

    // 获取描述字段名
    public static string GetDescriptionFieldName(string language)
    {
        return "DescriptionZh";
    }

    // 获取标题字段名
    public static string GetTitleFieldName(string language)
    {
        return "TitleZh";
    }

    // 获取内容字段名
    public static string GetContentFieldName(string language)
    {
        return "ContentZh";
    }
}
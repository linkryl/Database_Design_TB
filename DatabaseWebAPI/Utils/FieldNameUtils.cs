/*
 * Project Name:  DatabaseWebAPI
 * File Name:     FieldNameUtils.cs
 * File Function: 字段名工具函数
 * Author:        宠悦 | PetJoy 项目开发组
 * Update Date:   2024-07-25
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

    // 获取起源地字段名
    public static string GetOriginFieldName(string language)
    {
        return "OriginZh";
    }

    // 获取体型字段名
    public static string GetSizeFieldName(string language)
    {
        return language switch
        {
            "zh" => "SizeZh",
            "de" => "SizeDe",
            "en" => "SizeEn",
            "es" => "SizeEs",
            "fr" => "SizeFr",
            "it" => "SizeIt",
            "ja" => "SizeJa",
            "ko" => "SizeKo",
            "pt" => "SizePt",
            "ru" => "SizeRu",
            _ => "SizeZh"
        };
    }

    // 获取毛色字段名
    public static string GetCoatFieldName(string language)
    {
        return language switch
        {
            "zh" => "CoatZh",
            "de" => "CoatDe",
            "en" => "CoatEn",
            "es" => "CoatEs",
            "fr" => "CoatFr",
            "it" => "CoatIt",
            "ja" => "CoatJa",
            "ko" => "CoatKo",
            "pt" => "CoatPt",
            "ru" => "CoatRu",
            _ => "CoatZh"
        };
    }

    // 获取寿命字段名
    public static string GetLifespanFieldName(string language)
    {
        return language switch
        {
            "zh" => "LifespanZh",
            "de" => "LifespanDe",
            "en" => "LifespanEn",
            "es" => "LifespanEs",
            "fr" => "LifespanFr",
            "it" => "LifespanIt",
            "ja" => "LifespanJa",
            "ko" => "LifespanKo",
            "pt" => "LifespanPt",
            "ru" => "LifespanRu",
            _ => "LifespanZh"
        };
    }

    // 获取性情字段名
    public static string GetTemperamentFieldName(string language)
    {
        return language switch
        {
            "zh" => "TemperamentZh",
            "de" => "TemperamentDe",
            "en" => "TemperamentEn",
            "es" => "TemperamentEs",
            "fr" => "TemperamentFr",
            "it" => "TemperamentIt",
            "ja" => "TemperamentJa",
            "ko" => "TemperamentKo",
            "pt" => "TemperamentPt",
            "ru" => "TemperamentRu",
            _ => "TemperamentZh"
        };
    }

    // 获取饮食习惯字段名
    public static string GetDietFieldName(string language)
    {
        return language switch
        {
            "zh" => "DietZh",
            "de" => "DietDe",
            "en" => "DietEn",
            "es" => "DietEs",
            "fr" => "DietFr",
            "it" => "DietIt",
            "ja" => "DietJa",
            "ko" => "DietKo",
            "pt" => "DietPt",
            "ru" => "DietRu",
            _ => "DietZh"
        };
    }

    // 获取标题字段名
    public static string GetTitleFieldName(string language)
    {
        return language switch
        {
            "zh" => "TitleZh",
            "de" => "TitleDe",
            "en" => "TitleEn",
            "es" => "TitleEs",
            "fr" => "TitleFr",
            "it" => "TitleIt",
            "ja" => "TitleJa",
            "ko" => "TitleKo",
            "pt" => "TitlePt",
            "ru" => "TitleRu",
            _ => "TitleZh"
        };
    }

    // 获取内容字段名
    public static string GetContentFieldName(string language)
    {
        return language switch
        {
            "zh" => "ContentZh",
            "de" => "ContentDe",
            "en" => "ContentEn",
            "es" => "ContentEs",
            "fr" => "ContentFr",
            "it" => "ContentIt",
            "ja" => "ContentJa",
            "ko" => "ContentKo",
            "pt" => "ContentPt",
            "ru" => "ContentRu",
            _ => "ContentZh"
        };
    }
}
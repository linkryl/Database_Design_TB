/*
 * Project Name:  DatabaseWebAPI
 * File Name:     ExperienceUtils.cs
 * File Function: 经验值计算工具类
 * Author:        TreeHole开发组
 * Update Date:   2025-09-14
 * License:       Creative Commons Attribution 4.0 International License
 */

namespace DatabaseWebAPI.Utils;

public static class ExperienceUtils
{
    // 定义每级所需经验值（简单的线性增长）
    private static readonly int[] ExpPerLevel =
    {
        0,      // 1级 (0-99)
        100,    // 2级 (100-299)
        300,    // 3级 (300-599)
        600,    // 4级 (600-999)
        1000,   // 5级 (1000-1499)
        1500,   // 6级 (1500-2099)
        2100,   // 7级 (2100-2799)
        2800,   // 8级 (2800-3599)
        3600,   // 9级 (3600-4499)
        4500    // 10级 (4500+)
    };

    // 根据经验值计算当前等级
    public static int CalculateLevel(int experiencePoints)
    {
        for (int level = ExpPerLevel.Length - 1; level >= 0; level--)
        {
            if (experiencePoints >= ExpPerLevel[level])
                return level + 1;
        }
        return 1;
    }

    // 获取升级到下一级所需经验值
    public static int GetExpToNextLevel(int currentLevel, int currentExp)
    {
        if (currentLevel >= ExpPerLevel.Length)
            return 0; // 已满级

        return ExpPerLevel[currentLevel] - currentExp;
    }

    // 获取经验值进度百分比
    public static double GetProgressPercentage(int currentLevel, int currentExp)
    {
        if (currentLevel >= ExpPerLevel.Length)
            return 100;

        int currentLevelExp = ExpPerLevel[currentLevel - 1];
        int nextLevelExp = ExpPerLevel[currentLevel];
        int expInThisLevel = currentExp - currentLevelExp;
        int totalExpForLevel = nextLevelExp - currentLevelExp;

        return (double)expInThisLevel / totalExpForLevel * 100;
    }
}
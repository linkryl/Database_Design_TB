/*
 * Project Name:  DatabaseWebAPI
 * File Name:     OracleDbContext.cs
 * File Function: OracleDbContext 类
 * Author:        TreeHole开发组
 * Update Date:   2025-07-29
 * License:       Creative Commons Attribution 4.0 International License
 */

using DatabaseWebAPI.Models.TableModels;
using Microsoft.EntityFrameworkCore;

namespace DatabaseWebAPI.Data;

public class OracleDbContext(DbContextOptions<OracleDbContext> options) : DbContext(options)
{
    // 配置数据库上下文实体集
    public DbSet<DevelopmentTeam> DevelopmentTeamSet { get; set; }
    public DbSet<Post> PostSet { get; set; }
    public DbSet<PostCategory> PostCategorySet { get; set; }
    public DbSet<PostComment> PostCommentSet { get; set; }
    public DbSet<PostCommentDislike> PostCommentDislikeSet { get; set; }
    public DbSet<PostCommentLike> PostCommentLikeSet { get; set; }
    public DbSet<PostCommentReport> PostCommentReportSet { get; set; }
    public DbSet<PostDislike> PostDislikeSet { get; set; }
    public DbSet<PostFavorite> PostFavoriteSet { get; set; }
    public DbSet<PostLike> PostLikeSet { get; set; }
    public DbSet<PostReport> PostReportSet { get; set; }
    public DbSet<User> UserSet { get; set; }

    public DbSet<UserFollow> UserFollowSet { get; set; }
    public DbSet<UserMessage> UserMessageSet { get; set; }
    public DbSet<UserSetting> UserSettingSet { get; set; }

    // 重写 OnModelCreating 方法
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置复合主键
        modelBuilder.Entity<PostCommentDislike>().HasKey(n => new { n.CommentId, n.UserId });
        modelBuilder.Entity<PostCommentLike>().HasKey(n => new { n.CommentId, n.UserId });
        modelBuilder.Entity<PostDislike>().HasKey(n => new { n.PostId, n.UserId });
        modelBuilder.Entity<PostFavorite>().HasKey(n => new { n.PostId, n.UserId });
        modelBuilder.Entity<PostLike>().HasKey(n => new { n.PostId, n.UserId });
        modelBuilder.Entity<UserFollow>().HasKey(n => new { n.UserId, n.FollowerId });

        // 配置 POST_COMMENT_REPORT 与 USER 的关系
        modelBuilder.Entity<PostCommentReport>()
            .HasOne(n => n.Reporter)
            .WithMany(u => u.PostCommentReportEntityReporter)
            .HasForeignKey(n => n.ReporterId);
        modelBuilder.Entity<PostCommentReport>()
            .HasOne(n => n.ReportedUser)
            .WithMany(u => u.PostCommentReportEntityReportedUser)
            .HasForeignKey(n => n.ReportedUserId);

        // 配置 POST_REPORT 与 USER 的关系
        modelBuilder.Entity<PostReport>()
            .HasOne(n => n.Reporter)
            .WithMany(u => u.PostReportEntityReporter)
            .HasForeignKey(n => n.ReporterId);
        modelBuilder.Entity<PostReport>()
            .HasOne(n => n.ReportedUser)
            .WithMany(u => u.PostReportEntityReportedUser)
            .HasForeignKey(n => n.ReportedUserId);
        
    }
}
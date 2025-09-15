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
    public DbSet<Bar> BarSet { get; set; }
    public DbSet<BarFollow> BarFollowSet { get; set; }
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

    // 选举相关数据集
    public DbSet<BarElection> BarElectionSet { get; set; }
    public DbSet<BarElectionCandidate> BarElectionCandidateSet { get; set; }
    public DbSet<BarElectionVote> BarElectionVoteSet { get; set; }

    // 群组相关表
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<GroupMessage> GroupMessages { get; set; }

    // 重写 OnModelCreating 方法
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置复合主键
        modelBuilder.Entity<BarFollow>().HasKey(n => new { n.BarId, n.UserId });
        modelBuilder.Entity<PostCommentDislike>().HasKey(n => new { n.CommentId, n.UserId });
        modelBuilder.Entity<PostCommentLike>().HasKey(n => new { n.CommentId, n.UserId });
        modelBuilder.Entity<PostDislike>().HasKey(n => new { n.PostId, n.UserId });
        modelBuilder.Entity<PostFavorite>().HasKey(n => new { n.PostId, n.UserId });
        modelBuilder.Entity<PostLike>().HasKey(n => new { n.PostId, n.UserId });
        modelBuilder.Entity<UserFollow>().HasKey(n => new { n.UserId, n.FollowerId });

        // 唯一约束：
        // 1) 一人一票（同一场选举中同一用户仅能投一次）
        modelBuilder.Entity<BarElectionVote>()
            .HasIndex(v => new { v.ElectionId, v.VoterUserId })
            .IsUnique();
        // 2) 一人一次报名（同一场选举中同一用户仅能成为一个候选人）
        modelBuilder.Entity<BarElectionCandidate>()
            .HasIndex(c => new { c.ElectionId, c.UserId })
            .IsUnique();

        // 配置 POST_COMMENT_REPORT 与 USER 的关系
        modelBuilder.Entity<PostCommentReport>()
            .HasOne(n => n.Reporter)
            .WithMany(u => u.PostCommentReportEntityReporter)
            .HasForeignKey(n => n.ReporterId);
        modelBuilder.Entity<PostCommentReport>()
            .HasOne(n => n.ReportedUser)
            .WithMany(u => u.PostCommentReportEntityReportedUser)
            .HasForeignKey(n => n.ReportedUserId);

        // 配置群组相关表名和主键
        modelBuilder.Entity<Group>().ToTable("GROUP");
        modelBuilder.Entity<Group>().HasKey(g => g.GroupId);
        modelBuilder.Entity<Group>().Property(g => g.GroupId).HasColumnName("GROUP_ID").ValueGeneratedOnAdd();
        modelBuilder.Entity<Group>().Property(g => g.GroupName).HasColumnName("GROUP_NAME");
        modelBuilder.Entity<Group>().Property(g => g.GroupDesc).HasColumnName("GROUP_DESC");
        modelBuilder.Entity<Group>().Property(g => g.CreateUserId).HasColumnName("CREATE_USER_ID");
        modelBuilder.Entity<Group>().Property(g => g.CreateTime).HasColumnName("CREATE_TIME");
        modelBuilder.Entity<Group>().Property(g => g.LastActiveTime).HasColumnName("LAST_ACTIVE_TIME");
        modelBuilder.Entity<Group>().Property(g => g.MemberCount).HasColumnName("MEMBER_COUNT");

        modelBuilder.Entity<GroupMember>().ToTable("GROUP_MEMBER");
        modelBuilder.Entity<GroupMember>().HasKey(gm => gm.MemberId);
        modelBuilder.Entity<GroupMember>().Property(gm => gm.MemberId).HasColumnName("MEMBER_ID").ValueGeneratedOnAdd();
        modelBuilder.Entity<GroupMember>().Property(gm => gm.GroupId).HasColumnName("GROUP_ID");
        modelBuilder.Entity<GroupMember>().Property(gm => gm.UserId).HasColumnName("USER_ID");
        modelBuilder.Entity<GroupMember>().Property(gm => gm.JoinTime).HasColumnName("JOIN_TIME");
        modelBuilder.Entity<GroupMember>().Property(gm => gm.Role).HasColumnName("ROLE");
        modelBuilder.Entity<GroupMember>().Property(gm => gm.IsMuted).HasColumnName("IS_MUTED");

        modelBuilder.Entity<GroupMessage>().ToTable("GROUP_MESSAGE");
        modelBuilder.Entity<GroupMessage>().HasKey(gm => gm.MessageId);
        modelBuilder.Entity<GroupMessage>().Property(gm => gm.MessageId).HasColumnName("MESSAGE_ID").ValueGeneratedOnAdd();
        modelBuilder.Entity<GroupMessage>().Property(gm => gm.GroupId).HasColumnName("GROUP_ID");
        modelBuilder.Entity<GroupMessage>().Property(gm => gm.SenderId).HasColumnName("SENDER_ID");
        modelBuilder.Entity<GroupMessage>().Property(gm => gm.Content).HasColumnName("CONTENT");
        modelBuilder.Entity<GroupMessage>().Property(gm => gm.MessageType).HasColumnName("MESSAGE_TYPE");
        modelBuilder.Entity<GroupMessage>().Property(gm => gm.SendTime).HasColumnName("SEND_TIME");
        modelBuilder.Entity<GroupMessage>().Property(gm => gm.IsDeleted).HasColumnName("IS_DELETED");

        // 配置 POST_REPORT 与 USER 的关系
        modelBuilder.Entity<PostReport>()
            .HasOne(n => n.Reporter)
            .WithMany(u => u.PostReportEntityReporter)
            .HasForeignKey(n => n.ReporterId);
        modelBuilder.Entity<PostReport>()
            .HasOne(n => n.ReportedUser)
            .WithMany(u => u.PostReportEntityReportedUser)
            .HasForeignKey(n => n.ReportedUserId);

        // 暂时注释掉新字段配置，等字段确认存在后再启用
        // modelBuilder.Entity<Post>(entity =>
        // {
        //     entity.Property(e => e.BarId)
        //         .HasColumnName("BAR_ID")
        //         .HasColumnType("NUMBER");
        //         
        //     entity.Property(e => e.AlsoInTreehole)
        //         .HasColumnName("ALSO_IN_TREEHOLE")
        //         .HasColumnType("NUMBER")
        //         .HasDefaultValue(0);
        // });
        
    }
}
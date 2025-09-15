-- 用户表 USER 的数据定义语言
create table "USER"
(
    user_id             INT           GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) not null
        constraint USER_pk
            primary key,
    user_name           VARCHAR(64)   not null
        constraint user_name_uk
            unique,
    password            VARCHAR(64)   not null,
    telephone           VARCHAR(16)   not null
        constraint telephone_uk
            unique,
    registration_date   DATE          not null,
    last_login_time     DATE          not null,
    role                NUMBER(1)     not null,
    status              NUMBER(1)     not null,
    avatar_url          VARCHAR(2048),
    profile             VARCHAR(512),
    gender              NUMBER(1)     not null,
    birthdate           DATE          not null,
    experience_points   INT           not null,
    follows_count       INT           not null,
    followed_count      INT           not null,
    favorites_count     INT           not null,
    favorited_count     INT           not null,
    likes_count         INT           not null,
    liked_count         INT           not null,
    message_count       INT           not null
);

comment on table "USER" is '用户';
comment on column "USER".user_id is '用户ID';
comment on column "USER".user_name is '用户名';
comment on column "USER".password is '密码';
comment on column "USER".telephone is '手机号码';
comment on column "USER".registration_date is '注册日期';
comment on column "USER".last_login_time is '上次登录时间';
comment on column "USER".role is '用户角色';
comment on column "USER".status is '用户状态';
comment on column "USER".avatar_url is '头像链接';
comment on column "USER".profile is '个人简介';
comment on column "USER".gender is '性别';
comment on column "USER".birthdate is '出生日期';
comment on column "USER".experience_points is '经验点数';
comment on column "USER".follows_count is '关注数';
comment on column "USER".followed_count is '被关注数';
comment on column "USER".favorites_count is '收藏数';
comment on column "USER".favorited_count is '被收藏数';
comment on column "USER".likes_count is '点赞数';
comment on column "USER".liked_count is '被点赞数';
comment on column "USER".message_count is '留言数';

-- 用户设置表 USER_SETTING 的数据定义语言
create table USER_SETTING
(
    user_id                     INT       not null
        constraint USER_SETTING_pk
            primary key
        constraint USER_SETTING_USER_fk
            references "USER" ON DELETE CASCADE,
    is_telephone_public         NUMBER(1) not null,
    is_registration_date_public NUMBER(1) not null,
    is_profile_public           NUMBER(1) not null,
    is_gender_public            NUMBER(1) not null,
    is_birthdate_public         NUMBER(1) not null,
    is_level_public             NUMBER(1) not null,
    is_following_count_public   NUMBER(1) not null,
    is_followers_count_public   NUMBER(1) not null,
    is_favorites_count_public   NUMBER(1) not null,
    is_favored_count_public     NUMBER(1) not null,
    is_likes_count_public       NUMBER(1) not null,
    is_liked_count_public       NUMBER(1) not null,
    is_message_count_public     NUMBER(1) not null,
    allow_following             NUMBER(1) not null,
    receive_admin_notifications NUMBER(1) not null,
    receive_user_notifications  NUMBER(1) not null
);

comment on table USER_SETTING is '用户设置';
comment on column USER_SETTING.user_id is '用户ID';
comment on column USER_SETTING.is_telephone_public is '是否公开手机号码';
comment on column USER_SETTING.is_registration_date_public is '是否公开注册日期';
comment on column USER_SETTING.is_profile_public is '是否公开个人简介';
comment on column USER_SETTING.is_gender_public is '是否公开性别';
comment on column USER_SETTING.is_birthdate_public is '是否公开出生日期';
comment on column USER_SETTING.is_level_public is '是否公开等级';
comment on column USER_SETTING.is_following_count_public is '是否公开关注数';
comment on column USER_SETTING.is_followers_count_public is '是否公开被关注数';
comment on column USER_SETTING.is_favorites_count_public is '是否公开收藏数';
comment on column USER_SETTING.is_favored_count_public is '是否公开被收藏数';
comment on column USER_SETTING.is_likes_count_public is '是否公开点赞数';
comment on column USER_SETTING.is_liked_count_public is '是否公开被点赞数';
comment on column USER_SETTING.is_message_count_public is '是否公开留言数';
comment on column USER_SETTING.allow_following is '是否允许他人关注';
comment on column USER_SETTING.receive_admin_notifications is '是否接受管理员通知';
comment on column USER_SETTING.receive_user_notifications is '是否接受用户通知';

-- 用户打卡表 USER_CHECK_IN 的数据定义语言
create table USER_CHECK_IN
(
    check_in_id   INT  GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) not null
        constraint USER_CHECK_IN_pk
            primary key,
    user_id       INT  not null
        constraint USER_CHECK_IN_USER_fk
            references "USER" ON DELETE CASCADE,
    check_in_time DATE not null
);

comment on table USER_CHECK_IN is '用户打卡';
comment on column USER_CHECK_IN.check_in_id is '打卡记录ID';
comment on column USER_CHECK_IN.user_id is '用户ID';
comment on column USER_CHECK_IN.check_in_time is '打卡时间';

-- 用户关注表 USER_FOLLOW 的数据定义语言
create table USER_FOLLOW
(
    user_id     INT  not null
        constraint USER_FOLLOW_USER_fk_1
            references "USER" ON DELETE CASCADE,
    follower_id INT  not null
        constraint USER_FOLLOW_USER_fk_2
            references "USER" ON DELETE CASCADE,
    follow_time DATE not null,
    constraint USER_FOLLOW_pk
        primary key (user_id, follower_id)
);

comment on table USER_FOLLOW is '用户关注';
comment on column USER_FOLLOW.user_id is '被关注者ID';
comment on column USER_FOLLOW.follower_id is '关注者ID';
comment on column USER_FOLLOW.follow_time is '关注时间';

-- 用户留言表 USER_MESSAGE 的数据定义语言
create table USER_MESSAGE
(
    message_id   INT          GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) not null
        constraint USER_MESSAGE_pk
            primary key,
    user_id      INT          not null
        constraint USER_MESSAGE_USER_fk_1
            references "USER" ON DELETE CASCADE,
    commenter_id INT          not null
        constraint USER_MESSAGE_USER_fk_2
            references "USER" ON DELETE CASCADE,
    message      VARCHAR(512) not null,
    comment_time DATE         not null
);

comment on table USER_MESSAGE is '用户留言';
comment on column USER_MESSAGE.message_id is '留言ID';
comment on column USER_MESSAGE.user_id is '用户ID';
comment on column USER_MESSAGE.commenter_id is '留言者ID';
comment on column USER_MESSAGE.message is '留言内容';
comment on column USER_MESSAGE.comment_time is '留言时间';

-- 用户反馈表 USER_FEEDBACK 的数据定义语言
create table USER_FEEDBACK
(
    feedback_id       INTEGER       GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) not null
        constraint USER_FEEDBACK_pk
            primary key,
    feedback_category VARCHAR(32)   not null,
    feedback_content  VARCHAR(2048) not null,
    feedback_time     DATE          not null,
    email             VARCHAR(2048),
    telephone         VARCHAR(16)
);

comment on table USER_FEEDBACK is '用户反馈';
comment on column USER_FEEDBACK.feedback_id is '反馈意见ID';
comment on column USER_FEEDBACK.feedback_category is '反馈类型';
comment on column USER_FEEDBACK.feedback_content is '反馈内容';
comment on column USER_FEEDBACK.feedback_time is '反馈时间';
comment on column USER_FEEDBACK.email is '电子邮箱';
comment on column USER_FEEDBACK.telephone is '手机号码';

-- 帖子分类表 POST_CATEGORY 的数据定义语言
create table POST_CATEGORY
(
    category_id INT         not null
        constraint POST_CATEGORY_pk
            primary key,
    category    VARCHAR(64) not null
        constraint post_category_uk
            unique
);

comment on table POST_CATEGORY is '帖子分类';
comment on column POST_CATEGORY.category_id is '帖子分类ID';
comment on column POST_CATEGORY.category is '帖子分类';

-- 帖子表 POST 的数据定义语言
create table POST
(
    post_id        INT           GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) not null
        constraint POST_pk
            primary key,
    user_id        INT           not null
        constraint POST_USER_fk
            references "USER" ON DELETE CASCADE,
    category_id    INT           not null
        constraint POST_POST_CATEGORY_fk
            references POST_CATEGORY (category_id) ON DELETE CASCADE,
    title          VARCHAR(256)  not null,
    content        VARCHAR(2048) not null,
    creation_date  DATE          not null,
    update_date    DATE          not null,
    is_sticky      NUMBER(1)     not null,
    like_count     INT           not null,
    dislike_count  INT           not null,
    favorite_count INT           not null,
    comment_count  INT           not null,
    image_url      VARCHAR(2048)
);

comment on table POST is '帖子';
comment on column POST.post_id is '帖子ID';
comment on column POST.user_id is '发帖用户ID';
comment on column POST.category_id is '帖子分类ID';
comment on column POST.title is '标题';
comment on column POST.content is '内容';
comment on column POST.creation_date is '创建时间';
comment on column POST.update_date is '更新时间';
comment on column POST.is_sticky is '是否置顶';
comment on column POST.like_count is '点赞数';
comment on column POST.dislike_count is '点踩数';
comment on column POST.favorite_count is '收藏数';
comment on column POST.comment_count is '评论数';
comment on column POST.image_url is '图片链接';

-- 帖子评论表 POST_COMMENT 的数据定义语言
create table POST_COMMENT
(
    comment_id        INT          GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) not null
        constraint POST_COMMENT_pk
            primary key,
    post_id           INT          not null
        constraint POST_COMMENT_POST_fk
            references POST ON DELETE CASCADE,
    user_id           INT          not null
        constraint POST_COMMENT_USER_fk
            references "USER" ON DELETE CASCADE,
    parent_comment_id INT
        constraint POST_COMMENT_POST_COMMENT_fk
            references POST_COMMENT ON DELETE CASCADE,
    content           VARCHAR(512) not null,
    comment_time      DATE         not null,
    like_count        INT          not null,
    dislike_count     INT          not null
);

comment on table POST_COMMENT is '帖子评论';
comment on column POST_COMMENT.comment_id is '帖子评论ID';
comment on column POST_COMMENT.post_id is '帖子ID';
comment on column POST_COMMENT.user_id is '用户ID';
comment on column POST_COMMENT.parent_comment_id is '父评论ID';
comment on column POST_COMMENT.content is '评论内容';
comment on column POST_COMMENT.comment_time is '评论时间';
comment on column POST_COMMENT.like_count is '点赞数';
comment on column POST_COMMENT.dislike_count is '点踩数';

-- 帖子点赞表 POST_LIKE 的数据定义语言
create table POST_LIKE
(
    post_id   INT  not null
        constraint POST_LIKE_POST_fk
            references POST ON DELETE CASCADE,
    user_id   INT  not null
        constraint POST_LIKE_USER_fk
            references "USER" ON DELETE CASCADE,
    like_time DATE not null,
    constraint POST_LIKE_pk
        primary key (post_id, user_id)
);

comment on table POST_LIKE is '帖子点赞';
comment on column POST_LIKE.post_id is '帖子ID';
comment on column POST_LIKE.user_id is '用户ID';
comment on column POST_LIKE.like_time is '点赞时间';

-- 帖子点踩表 POST_DISLIKE 的数据定义语言
create table POST_DISLIKE
(
    post_id      INT  not null
        constraint POST_DISLIKE_POST_fk
            references POST ON DELETE CASCADE,
    user_id      INT  not null
        constraint POST_DISLIKE_USER_fk
            references "USER" ON DELETE CASCADE,
    dislike_time DATE not null,
    constraint POST_DISLIKE_pk
        primary key (post_id, user_id)
);

comment on table POST_DISLIKE is '帖子点踩';
comment on column POST_DISLIKE.post_id is '帖子ID';
comment on column POST_DISLIKE.user_id is '用户ID';
comment on column POST_DISLIKE.dislike_time is '点踩时间';

-- 帖子评论点赞表 POST_COMMENT_LIKE 的数据定义语言
create table POST_COMMENT_LIKE
(
    comment_id INT  not null
        constraint POST_COMMENT_LPC_fk
            references POST_COMMENT ON DELETE CASCADE,
    user_id    INT  not null
        constraint POST_COMMENT_LIKE_USER_fk
            references "USER" ON DELETE CASCADE,
    like_time  DATE not null,
    constraint POST_COMMENT_LIKE_pk
        primary key (comment_id, user_id)
);

comment on table POST_COMMENT_LIKE is '帖子评论点赞';
comment on column POST_COMMENT_LIKE.comment_id is '帖子评论ID';
comment on column POST_COMMENT_LIKE.user_id is '用户ID';
comment on column POST_COMMENT_LIKE.like_time is '点赞时间';

-- 帖子评论点踩表 POST_COMMENT_DISLIKE 的数据定义语言
create table POST_COMMENT_DISLIKE
(
    comment_id   INT  not null
        constraint POST_COMMENT_DPC_fk
            references POST_COMMENT ON DELETE CASCADE,
    user_id      INT  not null
        constraint POST_COMMENT_DISLIKE_USER_fk
            references "USER" ON DELETE CASCADE,
    dislike_time DATE not null,
    constraint POST_COMMENT_DISLIKE_pk
        primary key (comment_id, user_id)
);

comment on table POST_COMMENT_DISLIKE is '帖子评论点踩';
comment on column POST_COMMENT_DISLIKE.comment_id is '帖子评论ID';
comment on column POST_COMMENT_DISLIKE.user_id is '用户ID';
comment on column POST_COMMENT_DISLIKE.dislike_time is '点踩时间';

-- 帖子收藏表 POST_FAVORITE 的数据定义语言
create table POST_FAVORITE
(
    post_id       INT  not null
        constraint POST_FAVORITE_POST_fk
            references POST ON DELETE CASCADE,
    user_id       INT  not null
        constraint POST_FAVORITE_USER_fk
            references "USER" ON DELETE CASCADE,
    favorite_time DATE not null,
    constraint POST_FAVORITE_pk
        primary key (post_id, user_id)
);

comment on table POST_FAVORITE is '帖子收藏';
comment on column POST_FAVORITE.post_id is '帖子ID';
comment on column POST_FAVORITE.user_id is '用户ID';
comment on column POST_FAVORITE.favorite_time is '收藏时间';

-- 帖子举报表 POST_REPORT 的数据定义语言
create table POST_REPORT
(
    post_report_id   INT          GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) not null
        constraint POST_REPORT_pk
            primary key,
    reporter_id      INT          not null
        constraint POST_REPORT_USER_fk_1
            references "USER" ON DELETE CASCADE,
    reported_user_id INT          not null
        constraint POST_REPORT_USER_fk_2
            references "USER" ON DELETE CASCADE,
    reported_post_id INT          not null
        constraint POST_REPORT_POST_fk
            references POST ON DELETE CASCADE,
    report_reason    VARCHAR(512) not null,
    report_time      DATE         not null,
    status           NUMBER(1)    not null
);

comment on table POST_REPORT is '帖子举报';
comment on column POST_REPORT.post_report_id is '帖子举报ID';
comment on column POST_REPORT.reporter_id is '举报者ID';
comment on column POST_REPORT.reported_user_id is '被举报者ID';
comment on column POST_REPORT.reported_post_id is '被举报帖子ID';
comment on column POST_REPORT.report_reason is '举报原因';
comment on column POST_REPORT.report_time is '举报时间';
comment on column POST_REPORT.status is '处理状态';

-- 帖子评论举报表 POST_COMMENT_REPORT 的数据定义语言
create table POST_COMMENT_REPORT
(
    post_comment_report_id INT          GENERATED ALWAYS AS IDENTITY (START WITH 1 INCREMENT BY 1) not null
        constraint POST_COMMENT_REPORT_pk
            primary key,
    reporter_id            INT          not null
        constraint POST_COMMENT_REPORT_USER_fk_1
            references "USER" ON DELETE CASCADE,
    reported_user_id       INT          not null
        constraint POST_COMMENT_REPORT_USER_fk_2
            references "USER" ON DELETE CASCADE,
    reported_post_id       INT          not null
        constraint POST_COMMENT_REPORT_POST_fk
            references POST ON DELETE CASCADE,
    reported_comment_id    INT          not null
        constraint POST_COMMENT_REPORT_PC_fk
            references POST_COMMENT ON DELETE CASCADE,
    report_reason          VARCHAR(512) not null,
    report_time            DATE         not null,
    status                 NUMBER(1)    not null
);

comment on table POST_COMMENT_REPORT is '帖子评论举报';
comment on column POST_COMMENT_REPORT.post_comment_report_id is '帖子评论举报ID';
comment on column POST_COMMENT_REPORT.reporter_id is '举报者ID';
comment on column POST_COMMENT_REPORT.reported_user_id is '被举报者ID';
comment on column POST_COMMENT_REPORT.reported_post_id is '被举报评论所属帖子ID';
comment on column POST_COMMENT_REPORT.reported_comment_id is '被举报评论ID';
comment on column POST_COMMENT_REPORT.report_reason is '举报原因';
comment on column POST_COMMENT_REPORT.report_time is '举报时间';
comment on column POST_COMMENT_REPORT.status is '处理状态';

-- 开发团队表 DEVELOPMENT_TEAM 的数据定义语言
create table DEVELOPMENT_TEAM
(
    id             INT           not null
        constraint DEVELOPMENT_TEAM_pk
            primary key,
    name           VARCHAR(64)   not null,
    school         VARCHAR(64)   not null,
    email          VARCHAR(2048) not null,
    github_name    VARCHAR(64)   not null,
    github_profile VARCHAR(2048) not null
);

comment on table DEVELOPMENT_TEAM is '开发团队';
comment on column DEVELOPMENT_TEAM.id is '学号';
comment on column DEVELOPMENT_TEAM.name is '姓名';
comment on column DEVELOPMENT_TEAM.school is '学校';
comment on column DEVELOPMENT_TEAM.email is '电子邮箱';
comment on column DEVELOPMENT_TEAM.github_name is 'GitHub名称';
comment on column DEVELOPMENT_TEAM.github_profile is 'GitHub主页';

-- 建表脚本：吧主选举
-- ALTER SESSION SET CURRENT_SCHEMA = DBTEAM;

-- 为了可重复执行，先尝试删除可能已存在的外键与表（忽略不存在错误）
BEGIN EXECUTE IMMEDIATE 'ALTER TABLE BAR_ELECTION_VOTE DROP CONSTRAINT FK_VOTE_VOTER_USER'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -2443 THEN RAISE; END IF; END; /
BEGIN EXECUTE IMMEDIATE 'ALTER TABLE BAR_ELECTION_VOTE DROP CONSTRAINT FK_VOTE_CANDIDATE_USER'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -2443 THEN RAISE; END IF; END; /
BEGIN EXECUTE IMMEDIATE 'ALTER TABLE BAR_ELECTION_VOTE DROP CONSTRAINT FK_VOTE_ELECTION'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -2443 THEN RAISE; END IF; END; /
BEGIN EXECUTE IMMEDIATE 'ALTER TABLE BAR_ELECTION_CANDIDATE DROP CONSTRAINT FK_CANDIDATE_USER'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -2443 THEN RAISE; END IF; END; /
BEGIN EXECUTE IMMEDIATE 'ALTER TABLE BAR_ELECTION_CANDIDATE DROP CONSTRAINT FK_CANDIDATE_ELECTION'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -2443 THEN RAISE; END IF; END; /
BEGIN EXECUTE IMMEDIATE 'ALTER TABLE BAR_ELECTION DROP CONSTRAINT FK_ELECTION_BAR'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -2443 THEN RAISE; END IF; END; /

BEGIN EXECUTE IMMEDIATE 'DROP INDEX UQ_ELECTION_VOTE'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1418 AND SQLCODE != -1418 THEN NULL; END IF; END; /
BEGIN EXECUTE IMMEDIATE 'DROP INDEX UQ_ELECTION_CANDIDATE'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -1418 AND SQLCODE != -1418 THEN NULL; END IF; END; /

BEGIN EXECUTE IMMEDIATE 'DROP TABLE BAR_ELECTION_VOTE'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -942 THEN RAISE; END IF; END; /
BEGIN EXECUTE IMMEDIATE 'DROP TABLE BAR_ELECTION_CANDIDATE'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -942 THEN RAISE; END IF; END; /
BEGIN EXECUTE IMMEDIATE 'DROP TABLE BAR_ELECTION'; EXCEPTION WHEN OTHERS THEN IF SQLCODE != -942 THEN RAISE; END IF; END; /

-- 1) 选举主表
CREATE TABLE BAR_ELECTION (
  ELECTION_ID       NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  BAR_ID            NUMBER NOT NULL,
  STATUS            NUMBER NOT NULL,         -- 0未开始 1进行中 2已结束
  START_TIME        DATE NOT NULL,
  END_TIME          DATE NOT NULL,
  CREATOR_USER_ID   NUMBER NOT NULL
);

-- 2) 候选人表
CREATE TABLE BAR_ELECTION_CANDIDATE (
  CANDIDATE_ID      NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  ELECTION_ID       NUMBER NOT NULL,
  USER_ID           NUMBER NOT NULL,
  MANIFESTO         VARCHAR2(512),
  CREATE_TIME       DATE DEFAULT SYSDATE
);

-- 3) 投票表
CREATE TABLE BAR_ELECTION_VOTE (
  VOTE_ID           NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  ELECTION_ID       NUMBER NOT NULL,
  VOTER_USER_ID     NUMBER NOT NULL,
  CANDIDATE_USER_ID NUMBER NOT NULL,
  VOTE_TIME         DATE DEFAULT SYSDATE
);

-- 唯一约束
CREATE UNIQUE INDEX UQ_ELECTION_VOTE ON BAR_ELECTION_VOTE (ELECTION_ID, VOTER_USER_ID);
CREATE UNIQUE INDEX UQ_ELECTION_CANDIDATE ON BAR_ELECTION_CANDIDATE (ELECTION_ID, USER_ID);

-- 外键约束（表名需与现有表一致）
ALTER TABLE BAR_ELECTION
  ADD CONSTRAINT FK_ELECTION_BAR
  FOREIGN KEY (BAR_ID) REFERENCES BAR (BAR_ID);

ALTER TABLE BAR_ELECTION_CANDIDATE
  ADD CONSTRAINT FK_CANDIDATE_ELECTION
  FOREIGN KEY (ELECTION_ID) REFERENCES BAR_ELECTION (ELECTION_ID);

ALTER TABLE BAR_ELECTION_CANDIDATE
  ADD CONSTRAINT FK_CANDIDATE_USER
  FOREIGN KEY (USER_ID) REFERENCES "USER" (USER_ID);

ALTER TABLE BAR_ELECTION_VOTE
  ADD CONSTRAINT FK_VOTE_ELECTION
  FOREIGN KEY (ELECTION_ID) REFERENCES BAR_ELECTION (ELECTION_ID);

ALTER TABLE BAR_ELECTION_VOTE
  ADD CONSTRAINT FK_VOTE_CANDIDATE_USER
  FOREIGN KEY (CANDIDATE_USER_ID) REFERENCES "USER" (USER_ID);

ALTER TABLE BAR_ELECTION_VOTE
  ADD CONSTRAINT FK_VOTE_VOTER_USER
  FOREIGN KEY (VOTER_USER_ID) REFERENCES "USER" (USER_ID);

-- 提交
COMMIT;

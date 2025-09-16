-- 内容标签和分类系统
-- 仿百度贴吧的内容组织方式

-- 话题标签表
CREATE TABLE TOPIC_TAG (
    tag_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tag_name VARCHAR(64) NOT NULL UNIQUE,
    tag_description VARCHAR(256),
    tag_color VARCHAR(16) DEFAULT '#409eff',
    tag_icon VARCHAR(32),
    post_count INT DEFAULT 0,
    follower_count INT DEFAULT 0,
    is_hot NUMBER(1) DEFAULT 0, -- 是否为热门话题
    is_official NUMBER(1) DEFAULT 0, -- 是否为官方话题
    created_time DATE DEFAULT CURRENT_DATE,
    last_active_time DATE DEFAULT CURRENT_DATE
);

-- 帖子标签关联表
CREATE TABLE POST_TAG_RELATION (
    relation_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    post_id INT NOT NULL,
    tag_id INT NOT NULL,
    created_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (post_id) REFERENCES POST(POST_ID) ON DELETE CASCADE,
    FOREIGN KEY (tag_id) REFERENCES TOPIC_TAG(TAG_ID) ON DELETE CASCADE,
    UNIQUE(post_id, tag_id)
);

-- 用户关注话题表
CREATE TABLE USER_TOPIC_FOLLOW (
    follow_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    tag_id INT NOT NULL,
    follow_time DATE DEFAULT CURRENT_DATE,
    is_active NUMBER(1) DEFAULT 1,
    FOREIGN KEY (user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE,
    FOREIGN KEY (tag_id) REFERENCES TOPIC_TAG(TAG_ID) ON DELETE CASCADE,
    UNIQUE(user_id, tag_id)
);

-- 热门话题排行表
CREATE TABLE HOT_TOPIC_RANKING (
    ranking_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    tag_id INT NOT NULL,
    rank_position INT NOT NULL,
    heat_score DECIMAL(10,2) DEFAULT 0,
    post_count_today INT DEFAULT 0,
    ranking_date DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (tag_id) REFERENCES TOPIC_TAG(TAG_ID) ON DELETE CASCADE,
    UNIQUE(tag_id, ranking_date)
);

-- 帖子质量评分表
CREATE TABLE POST_QUALITY_SCORE (
    score_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    post_id INT NOT NULL,
    quality_score DECIMAL(5,2) DEFAULT 0, -- 质量分数 0-100
    view_score DECIMAL(5,2) DEFAULT 0, -- 浏览量分数
    interaction_score DECIMAL(5,2) DEFAULT 0, -- 互动分数
    time_score DECIMAL(5,2) DEFAULT 0, -- 时效性分数
    final_score DECIMAL(5,2) DEFAULT 0, -- 最终综合分数
    is_featured NUMBER(1) DEFAULT 0, -- 是否为精华帖
    featured_time DATE, -- 加精时间
    calculated_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (post_id) REFERENCES POST(POST_ID) ON DELETE CASCADE,
    UNIQUE(post_id, calculated_time)
);

-- 用户兴趣标签表（用于推荐算法）
CREATE TABLE USER_INTEREST_TAG (
    interest_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    tag_id INT NOT NULL,
    interest_score DECIMAL(5,2) DEFAULT 0, -- 兴趣分数 0-100
    view_count INT DEFAULT 0, -- 查看该标签帖子的次数
    like_count INT DEFAULT 0, -- 点赞该标签帖子的次数
    comment_count INT DEFAULT 0, -- 评论该标签帖子的次数
    last_interaction_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE,
    FOREIGN KEY (tag_id) REFERENCES TOPIC_TAG(TAG_ID) ON DELETE CASCADE,
    UNIQUE(user_id, tag_id)
);

-- 帖子推荐记录表
CREATE TABLE POST_RECOMMENDATION (
    recommendation_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    post_id INT NOT NULL,
    recommendation_score DECIMAL(5,2) DEFAULT 0,
    recommendation_reason VARCHAR(256), -- 推荐理由
    is_clicked NUMBER(1) DEFAULT 0, -- 是否被点击
    click_time DATE,
    created_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE,
    FOREIGN KEY (post_id) REFERENCES POST(POST_ID) ON DELETE CASCADE
);

-- 插入默认话题标签
INSERT INTO TOPIC_TAG (tag_name, tag_description, tag_color, tag_icon, is_official) VALUES
('日常分享', '分享生活中的点点滴滴', '#ff9a9e', '📝', 1),
('学习交流', '学习心得和经验分享', '#409eff', '📚', 1),
('技术讨论', '技术问题和解决方案', '#67c23a', '💻', 1),
('校园生活', '校园趣事和学习生活', '#e6a23c', '🏫', 1),
('情感倾诉', '情感问题和心理健康', '#f56c6c', '💭', 1),
('兴趣爱好', '各种兴趣爱好分享', '#909399', '🎨', 1),
('求助问答', '寻求帮助和解答疑问', '#f78989', '❓', 1),
('活动公告', '官方活动和重要公告', '#5cb87a', '📢', 1);

-- 创建索引优化查询
CREATE INDEX idx_topic_tag_name ON TOPIC_TAG(TAG_NAME);
CREATE INDEX idx_topic_tag_hot ON TOPIC_TAG(IS_HOT, POST_COUNT DESC);
CREATE INDEX idx_post_tag_relation_post ON POST_TAG_RELATION(POST_ID);
CREATE INDEX idx_post_tag_relation_tag ON POST_TAG_RELATION(TAG_ID);
CREATE INDEX idx_user_topic_follow_user ON USER_TOPIC_FOLLOW(USER_ID);
CREATE INDEX idx_user_topic_follow_tag ON USER_TOPIC_FOLLOW(TAG_ID);
CREATE INDEX idx_post_quality_score_featured ON POST_QUALITY_SCORE(IS_FEATURED, FINAL_SCORE DESC);
CREATE INDEX idx_post_quality_score_score ON POST_QUALITY_SCORE(FINAL_SCORE DESC);
CREATE INDEX idx_user_interest_tag_user ON USER_INTEREST_TAG(USER_ID, INTEREST_SCORE DESC);
CREATE INDEX idx_post_recommendation_user ON POST_RECOMMENDATION(USER_ID, RECOMMENDATION_SCORE DESC);

-- 添加表注释
COMMENT ON TABLE TOPIC_TAG IS '话题标签表';
COMMENT ON TABLE POST_TAG_RELATION IS '帖子标签关联表';
COMMENT ON TABLE USER_TOPIC_FOLLOW IS '用户关注话题表';
COMMENT ON TABLE HOT_TOPIC_RANKING IS '热门话题排行表';
COMMENT ON TABLE POST_QUALITY_SCORE IS '帖子质量评分表';
COMMENT ON TABLE USER_INTEREST_TAG IS '用户兴趣标签表';
COMMENT ON TABLE POST_RECOMMENDATION IS '帖子推荐记录表';

-- 创建触发器：更新话题热度
CREATE OR REPLACE TRIGGER trg_update_topic_heat
AFTER INSERT ON POST_TAG_RELATION
FOR EACH ROW
BEGIN
    -- 更新话题的帖子数量
    UPDATE TOPIC_TAG 
    SET post_count = post_count + 1,
        last_active_time = CURRENT_DATE
    WHERE tag_id = :NEW.tag_id;
END;
/

-- 创建触发器：计算帖子质量分数
CREATE OR REPLACE TRIGGER trg_calculate_post_quality
AFTER UPDATE OF LIKE_COUNT, COMMENT_COUNT, FAVORITE_COUNT ON POST
FOR EACH ROW
DECLARE
    v_view_score NUMBER;
    v_interaction_score NUMBER;
    v_time_score NUMBER;
    v_final_score NUMBER;
    v_days_old NUMBER;
BEGIN
    -- 计算天数
    v_days_old := TRUNC(SYSDATE - :NEW.CREATION_DATE);
    
    -- 计算各项分数
    v_view_score := LEAST((:NEW.LIKE_COUNT + :NEW.COMMENT_COUNT + :NEW.FAVORITE_COUNT) * 2, 100);
    v_interaction_score := LEAST((:NEW.LIKE_COUNT * 1 + :NEW.COMMENT_COUNT * 2 + :NEW.FAVORITE_COUNT * 1.5), 100);
    v_time_score := GREATEST(100 - v_days_old * 2, 0);
    
    -- 计算最终分数
    v_final_score := (v_view_score * 0.3 + v_interaction_score * 0.5 + v_time_score * 0.2);
    
    -- 插入或更新质量分数记录
    MERGE INTO POST_QUALITY_SCORE pqs
    USING (SELECT :NEW.POST_ID as post_id FROM DUAL) src
    ON (pqs.post_id = src.post_id)
    WHEN MATCHED THEN
        UPDATE SET 
            quality_score = v_final_score,
            view_score = v_view_score,
            interaction_score = v_interaction_score,
            time_score = v_time_score,
            final_score = v_final_score,
            calculated_time = CURRENT_DATE
    WHEN NOT MATCHED THEN
        INSERT (post_id, quality_score, view_score, interaction_score, time_score, final_score)
        VALUES (:NEW.POST_ID, v_final_score, v_view_score, v_interaction_score, v_time_score, v_final_score);
END;
/

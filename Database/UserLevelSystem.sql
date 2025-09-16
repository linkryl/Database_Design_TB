-- 用户等级系统数据库设计
-- TreeHole论坛用户等级和成长体系

-- 用户等级配置表
CREATE TABLE USER_LEVEL_CONFIG (
    level_id INT PRIMARY KEY,
    level_name VARCHAR(32) NOT NULL,
    min_experience INT NOT NULL,
    max_experience INT NOT NULL,
    level_icon VARCHAR(256),
    level_color VARCHAR(16) DEFAULT '#409eff',
    privileges TEXT, -- JSON格式存储特权
    daily_post_limit INT DEFAULT 10, -- 每日发帖限制
    daily_comment_limit INT DEFAULT 50, -- 每日评论限制
    can_create_bar NUMBER(1) DEFAULT 0, -- 是否可创建贴吧
    can_pin_post NUMBER(1) DEFAULT 0, -- 是否可置顶帖子
    storage_quota BIGINT DEFAULT 104857600, -- 存储配额(字节)，默认100MB
    created_time DATE DEFAULT CURRENT_DATE
);

-- 插入等级配置数据
INSERT INTO USER_LEVEL_CONFIG VALUES (1, '新手', 0, 99, '🌱', '#67c23a', '{"title": "论坛新人", "description": "欢迎来到树洞论坛！"}', 5, 20, 0, 0, 52428800, CURRENT_DATE);
INSERT INTO USER_LEVEL_CONFIG VALUES (2, '初学者', 100, 299, '🌿', '#409eff', '{"title": "初出茅庐", "description": "开始熟悉论坛环境"}', 8, 30, 0, 0, 104857600, CURRENT_DATE);
INSERT INTO USER_LEVEL_CONFIG VALUES (3, '入门者', 300, 699, '🍀', '#409eff', '{"title": "渐入佳境", "description": "已经掌握基本操作"}', 10, 40, 0, 0, 209715200, CURRENT_DATE);
INSERT INTO USER_LEVEL_CONFIG VALUES (4, '熟练者', 700, 1499, '🌳', '#e6a23c', '{"title": "得心应手", "description": "论坛操作已经很熟练"}', 15, 50, 1, 0, 314572800, CURRENT_DATE);
INSERT INTO USER_LEVEL_CONFIG VALUES (5, '专家', 1500, 2999, '🌲', '#e6a23c', '{"title": "论坛专家", "description": "在某个领域有专业见解"}', 20, 60, 1, 0, 524288000, CURRENT_DATE);
INSERT INTO USER_LEVEL_CONFIG VALUES (6, '大师', 3000, 5999, '🎋', '#f56c6c', '{"title": "论坛大师", "description": "深受大家尊敬的用户"}', 25, 80, 1, 1, 1073741824, CURRENT_DATE);
INSERT INTO USER_LEVEL_CONFIG VALUES (7, '传奇', 6000, 9999, '🌺', '#f56c6c', '{"title": "传奇人物", "description": "论坛的传奇用户"}', 30, 100, 1, 1, 2147483648, CURRENT_DATE);
INSERT INTO USER_LEVEL_CONFIG VALUES (8, '神话', 10000, 99999, '👑', '#c71585', '{"title": "神话级用户", "description": "论坛最高荣誉"}', 50, 200, 1, 1, 5368709120, CURRENT_DATE);

-- 用户勋章表
CREATE TABLE USER_BADGE (
    badge_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    badge_name VARCHAR(64) NOT NULL,
    badge_desc VARCHAR(256),
    badge_icon VARCHAR(256),
    badge_type INT NOT NULL, -- 1:活跃度 2:贡献度 3:特殊成就 4:等级勋章 5:节日勋章
    badge_rarity INT DEFAULT 1, -- 稀有度：1普通 2稀有 3史诗 4传说
    unlock_condition TEXT, -- JSON格式解锁条件
    is_active NUMBER(1) DEFAULT 1,
    created_time DATE DEFAULT CURRENT_DATE
);

-- 插入勋章数据
INSERT INTO USER_BADGE (badge_name, badge_desc, badge_icon, badge_type, badge_rarity, unlock_condition) VALUES
('新人报到', '完成首次登录', '🎉', 1, 1, '{"type": "first_login"}'),
('初次发帖', '发布第一个帖子', '📝', 2, 1, '{"type": "first_post"}'),
('活跃用户', '连续签到7天', '🔥', 1, 2, '{"type": "consecutive_checkin", "days": 7}'),
('人气之星', '单个帖子获得100个赞', '⭐', 2, 2, '{"type": "post_likes", "count": 100}'),
('评论达人', '发表100条评论', '💬', 2, 2, '{"type": "comment_count", "count": 100}'),
('收藏家', '收藏50个帖子', '📚', 1, 2, '{"type": "favorite_count", "count": 50}'),
('社交达人', '关注50个用户', '👥', 1, 2, '{"type": "following_count", "count": 50}'),
('贴吧创建者', '创建第一个贴吧', '🏠', 3, 3, '{"type": "create_bar"}'),
('论坛元老', '注册满一年', '🏆', 3, 3, '{"type": "registration_days", "days": 365}'),
('传说用户', '达到传奇等级', '👑', 4, 4, '{"type": "user_level", "level": 7}');

-- 用户勋章关联表
CREATE TABLE USER_BADGE_RELATION (
    relation_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    badge_id INT NOT NULL,
    earned_date DATE DEFAULT CURRENT_DATE,
    is_displayed NUMBER(1) DEFAULT 1, -- 是否在个人资料中显示
    FOREIGN KEY (user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE,
    FOREIGN KEY (badge_id) REFERENCES USER_BADGE(BADGE_ID) ON DELETE CASCADE,
    UNIQUE(user_id, badge_id)
);

-- 用户签到表
CREATE TABLE USER_CHECK_IN (
    check_in_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    check_in_date DATE DEFAULT CURRENT_DATE,
    consecutive_days INT DEFAULT 1, -- 连续签到天数
    experience_gained INT DEFAULT 5, -- 获得的经验值
    bonus_applied NUMBER(1) DEFAULT 0, -- 是否应用了连续签到奖励
    created_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE,
    UNIQUE(user_id, check_in_date)
);

-- 经验值获取记录表
CREATE TABLE USER_EXPERIENCE_LOG (
    log_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    experience_change INT NOT NULL, -- 经验值变化（正数为获得，负数为扣除）
    action_type VARCHAR(32) NOT NULL, -- 行为类型：POST, COMMENT, LIKE_RECEIVED, CHECK_IN, etc.
    action_description VARCHAR(256), -- 行为描述
    related_id INT, -- 关联的帖子/评论等ID
    created_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE
);

-- 用户等级变更记录表
CREATE TABLE USER_LEVEL_LOG (
    log_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    old_level INT NOT NULL,
    new_level INT NOT NULL,
    experience_at_change INT NOT NULL, -- 升级时的经验值
    upgrade_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE
);

-- 用户任务系统表
CREATE TABLE USER_TASK (
    task_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    task_name VARCHAR(64) NOT NULL,
    task_desc VARCHAR(256),
    task_type INT NOT NULL, -- 1:每日任务 2:每周任务 3:成长任务 4:限时任务
    task_condition TEXT NOT NULL, -- JSON格式任务条件
    experience_reward INT DEFAULT 0, -- 经验值奖励
    badge_reward INT, -- 勋章奖励ID
    other_rewards TEXT, -- JSON格式其他奖励
    is_active NUMBER(1) DEFAULT 1,
    start_time DATE,
    end_time DATE,
    created_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (badge_reward) REFERENCES USER_BADGE(BADGE_ID)
);

-- 插入任务数据
INSERT INTO USER_TASK (task_name, task_desc, task_type, task_condition, experience_reward) VALUES
('每日签到', '每天签到一次', 1, '{"type": "daily_checkin", "count": 1}', 5),
('每日发帖', '每天发布1个帖子', 1, '{"type": "daily_post", "count": 1}', 10),
('每日评论', '每天发表3条评论', 1, '{"type": "daily_comment", "count": 3}', 8),
('每日点赞', '每天给别人点赞5次', 1, '{"type": "daily_like", "count": 5}', 3),
('周活跃用户', '一周内发帖和评论总数达到10次', 2, '{"type": "weekly_activity", "count": 10}', 50),
('新手引导', '完成个人资料设置', 3, '{"type": "complete_profile"}', 20),
('社交新手', '关注10个用户', 3, '{"type": "follow_users", "count": 10}', 30);

-- 用户任务进度表
CREATE TABLE USER_TASK_PROGRESS (
    progress_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    task_id INT NOT NULL,
    current_progress INT DEFAULT 0, -- 当前进度
    target_progress INT NOT NULL, -- 目标进度
    is_completed NUMBER(1) DEFAULT 0, -- 是否完成
    completed_time DATE, -- 完成时间
    reset_time DATE, -- 重置时间（用于每日/每周任务）
    created_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE,
    FOREIGN KEY (task_id) REFERENCES USER_TASK(TASK_ID) ON DELETE CASCADE,
    UNIQUE(user_id, task_id, reset_time)
);

-- 创建索引优化查询性能
CREATE INDEX idx_user_level_experience ON "USER"(EXPERIENCE_POINTS);
CREATE INDEX idx_user_badge_relation_user ON USER_BADGE_RELATION(USER_ID);
CREATE INDEX idx_user_check_in_user_date ON USER_CHECK_IN(USER_ID, CHECK_IN_DATE);
CREATE INDEX idx_user_experience_log_user_time ON USER_EXPERIENCE_LOG(USER_ID, CREATED_TIME);
CREATE INDEX idx_user_task_progress_user_task ON USER_TASK_PROGRESS(USER_ID, TASK_ID);

-- 添加表注释
COMMENT ON TABLE USER_LEVEL_CONFIG IS '用户等级配置表';
COMMENT ON TABLE USER_BADGE IS '用户勋章表';
COMMENT ON TABLE USER_BADGE_RELATION IS '用户勋章关联表';
COMMENT ON TABLE USER_CHECK_IN IS '用户签到记录表';
COMMENT ON TABLE USER_EXPERIENCE_LOG IS '用户经验值获取记录表';
COMMENT ON TABLE USER_LEVEL_LOG IS '用户等级变更记录表';
COMMENT ON TABLE USER_TASK IS '用户任务配置表';
COMMENT ON TABLE USER_TASK_PROGRESS IS '用户任务进度表';

-- 添加字段注释
COMMENT ON COLUMN USER_LEVEL_CONFIG.level_id IS '等级ID';
COMMENT ON COLUMN USER_LEVEL_CONFIG.level_name IS '等级名称';
COMMENT ON COLUMN USER_LEVEL_CONFIG.min_experience IS '最小经验值';
COMMENT ON COLUMN USER_LEVEL_CONFIG.max_experience IS '最大经验值';
COMMENT ON COLUMN USER_LEVEL_CONFIG.privileges IS '等级特权JSON配置';

COMMENT ON COLUMN USER_BADGE.badge_type IS '勋章类型：1活跃度 2贡献度 3特殊成就 4等级勋章 5节日勋章';
COMMENT ON COLUMN USER_BADGE.badge_rarity IS '稀有度：1普通 2稀有 3史诗 4传说';

COMMENT ON COLUMN USER_EXPERIENCE_LOG.experience_change IS '经验值变化（正数获得，负数扣除）';
COMMENT ON COLUMN USER_EXPERIENCE_LOG.action_type IS '行为类型：POST, COMMENT, LIKE_RECEIVED, CHECK_IN等';

-- 创建触发器：用户经验值变化时自动检查等级升级
CREATE OR REPLACE TRIGGER trg_user_level_check
AFTER UPDATE OF EXPERIENCE_POINTS ON "USER"
FOR EACH ROW
WHEN (NEW.EXPERIENCE_POINTS != OLD.EXPERIENCE_POINTS)
DECLARE
    v_new_level NUMBER;
    v_old_level NUMBER;
BEGIN
    -- 计算新等级
    SELECT level_id INTO v_new_level
    FROM USER_LEVEL_CONFIG
    WHERE :NEW.EXPERIENCE_POINTS >= min_experience 
    AND :NEW.EXPERIENCE_POINTS <= max_experience;
    
    -- 计算旧等级
    SELECT level_id INTO v_old_level
    FROM USER_LEVEL_CONFIG
    WHERE :OLD.EXPERIENCE_POINTS >= min_experience 
    AND :OLD.EXPERIENCE_POINTS <= max_experience;
    
    -- 如果等级发生变化，记录等级变更日志
    IF v_new_level != v_old_level THEN
        INSERT INTO USER_LEVEL_LOG (user_id, old_level, new_level, experience_at_change)
        VALUES (:NEW.USER_ID, v_old_level, v_new_level, :NEW.EXPERIENCE_POINTS);
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        NULL; -- 忽略找不到等级的情况
END;
/

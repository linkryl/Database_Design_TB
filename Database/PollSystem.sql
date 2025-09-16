-- 投票系统数据库设计
-- TreeHole论坛投票功能

-- 投票表
CREATE TABLE POLL (
    poll_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    post_id INT NOT NULL,
    poll_title VARCHAR(200) NOT NULL,
    poll_description VARCHAR(500),
    poll_type INT NOT NULL, -- 1:单选 2:多选
    allow_change_vote NUMBER(1) DEFAULT 1, -- 是否允许修改投票
    show_results NUMBER(1) DEFAULT 1, -- 是否显示结果
    deadline_time DATE, -- 截止时间
    total_votes INT DEFAULT 0, -- 总投票数
    is_active NUMBER(1) DEFAULT 1, -- 是否激活
    created_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (post_id) REFERENCES POST(POST_ID) ON DELETE CASCADE
);

-- 投票选项表
CREATE TABLE POLL_OPTION (
    option_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    poll_id INT NOT NULL,
    option_text VARCHAR(200) NOT NULL,
    option_description VARCHAR(300),
    vote_count INT DEFAULT 0,
    sort_order INT DEFAULT 0,
    FOREIGN KEY (poll_id) REFERENCES POLL(POLL_ID) ON DELETE CASCADE
);

-- 用户投票记录表
CREATE TABLE USER_POLL_VOTE (
    vote_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    poll_id INT NOT NULL,
    user_id INT NOT NULL,
    option_id INT NOT NULL,
    vote_time DATE DEFAULT CURRENT_DATE,
    ip_address VARCHAR(45),
    user_agent VARCHAR(500),
    FOREIGN KEY (poll_id) REFERENCES POLL(POLL_ID) ON DELETE CASCADE,
    FOREIGN KEY (user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE,
    FOREIGN KEY (option_id) REFERENCES POLL_OPTION(OPTION_ID) ON DELETE CASCADE
);

-- 创建复合索引
CREATE INDEX idx_user_poll_vote_user_poll ON USER_POLL_VOTE(USER_ID, POLL_ID);
CREATE INDEX idx_user_poll_vote_poll_option ON USER_POLL_VOTE(POLL_ID, OPTION_ID);
CREATE INDEX idx_poll_post ON POLL(POST_ID);
CREATE INDEX idx_poll_option_poll ON POLL_OPTION(POLL_ID, SORT_ORDER);
CREATE INDEX idx_poll_active_deadline ON POLL(IS_ACTIVE, DEADLINE_TIME);

-- 添加表注释
COMMENT ON TABLE POLL IS '投票表';
COMMENT ON COLUMN POLL.poll_id IS '投票ID';
COMMENT ON COLUMN POLL.post_id IS '关联帖子ID';
COMMENT ON COLUMN POLL.poll_title IS '投票标题';
COMMENT ON COLUMN POLL.poll_type IS '投票类型：1单选 2多选';
COMMENT ON COLUMN POLL.allow_change_vote IS '是否允许修改投票';
COMMENT ON COLUMN POLL.show_results IS '是否显示投票结果';
COMMENT ON COLUMN POLL.deadline_time IS '投票截止时间';

COMMENT ON TABLE POLL_OPTION IS '投票选项表';
COMMENT ON COLUMN POLL_OPTION.option_id IS '选项ID';
COMMENT ON COLUMN POLL_OPTION.poll_id IS '投票ID';
COMMENT ON COLUMN POLL_OPTION.option_text IS '选项文本';
COMMENT ON COLUMN POLL_OPTION.vote_count IS '得票数';

COMMENT ON TABLE USER_POLL_VOTE IS '用户投票记录表';
COMMENT ON COLUMN USER_POLL_VOTE.vote_id IS '投票记录ID';
COMMENT ON COLUMN USER_POLL_VOTE.poll_id IS '投票ID';
COMMENT ON COLUMN USER_POLL_VOTE.user_id IS '投票用户ID';
COMMENT ON COLUMN USER_POLL_VOTE.option_id IS '选择的选项ID';

-- 创建触发器：更新投票统计
CREATE OR REPLACE TRIGGER trg_update_poll_stats
AFTER INSERT ON USER_POLL_VOTE
FOR EACH ROW
BEGIN
    -- 更新选项得票数
    UPDATE POLL_OPTION 
    SET vote_count = vote_count + 1
    WHERE option_id = :NEW.option_id;
    
    -- 更新投票总数
    UPDATE POLL 
    SET total_votes = (
        SELECT COUNT(DISTINCT user_id) 
        FROM USER_POLL_VOTE 
        WHERE poll_id = :NEW.poll_id
    )
    WHERE poll_id = :NEW.poll_id;
END;
/

-- 创建触发器：删除投票时更新统计
CREATE OR REPLACE TRIGGER trg_update_poll_stats_delete
AFTER DELETE ON USER_POLL_VOTE
FOR EACH ROW
BEGIN
    -- 更新选项得票数
    UPDATE POLL_OPTION 
    SET vote_count = vote_count - 1
    WHERE option_id = :OLD.option_id;
    
    -- 更新投票总数
    UPDATE POLL 
    SET total_votes = (
        SELECT COUNT(DISTINCT user_id) 
        FROM USER_POLL_VOTE 
        WHERE poll_id = :OLD.poll_id
    )
    WHERE poll_id = :OLD.poll_id;
END;
/

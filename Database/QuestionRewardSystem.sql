-- 悬赏问答系统数据库设计
-- TreeHole论坛悬赏问答功能

-- 悬赏问题表
CREATE TABLE QUESTION_REWARD (
    question_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    post_id INT NOT NULL,
    questioner_id INT NOT NULL,
    reward_points INT NOT NULL, -- 悬赏积分
    question_type INT DEFAULT 1, -- 1:技术问题 2:学习问题 3:生活问题 4:其他
    difficulty_level INT DEFAULT 1, -- 难度等级 1-5
    expected_answer_type INT DEFAULT 1, -- 期望答案类型：1文字 2图片 3视频 4文档
    is_urgent NUMBER(1) DEFAULT 0, -- 是否紧急
    deadline_time DATE, -- 截止时间
    status INT DEFAULT 1, -- 1:进行中 2:已解决 3:已截止 4:已取消
    best_answer_id INT, -- 最佳答案ID
    answer_count INT DEFAULT 0, -- 回答数量
    view_count INT DEFAULT 0, -- 查看次数
    created_time DATE DEFAULT CURRENT_DATE,
    resolved_time DATE, -- 解决时间
    FOREIGN KEY (post_id) REFERENCES POST(POST_ID) ON DELETE CASCADE,
    FOREIGN KEY (questioner_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE
);

-- 问题回答表
CREATE TABLE QUESTION_ANSWER (
    answer_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    question_id INT NOT NULL,
    answerer_id INT NOT NULL,
    answer_content VARCHAR(2048) NOT NULL,
    answer_type INT DEFAULT 1, -- 1:文字 2:图片 3:视频 4:文档
    attachment_urls TEXT, -- JSON格式的附件URL列表
    is_best_answer NUMBER(1) DEFAULT 0, -- 是否为最佳答案
    helpful_count INT DEFAULT 0, -- 有用票数
    unhelpful_count INT DEFAULT 0, -- 无用票数
    comment_count INT DEFAULT 0, -- 评论数
    created_time DATE DEFAULT CURRENT_DATE,
    adopted_time DATE, -- 被采纳时间
    reward_received INT DEFAULT 0, -- 获得的悬赏积分
    FOREIGN KEY (question_id) REFERENCES QUESTION_REWARD(QUESTION_ID) ON DELETE CASCADE,
    FOREIGN KEY (answerer_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE
);

-- 答案评价表（用户对答案的有用/无用投票）
CREATE TABLE ANSWER_EVALUATION (
    evaluation_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    answer_id INT NOT NULL,
    evaluator_id INT NOT NULL,
    evaluation_type INT NOT NULL, -- 1:有用 0:无用
    evaluation_time DATE DEFAULT CURRENT_DATE,
    FOREIGN KEY (answer_id) REFERENCES QUESTION_ANSWER(ANSWER_ID) ON DELETE CASCADE,
    FOREIGN KEY (evaluator_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE,
    UNIQUE(answer_id, evaluator_id)
);

-- 悬赏积分交易记录表
CREATE TABLE REWARD_TRANSACTION (
    transaction_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    question_id INT NOT NULL,
    from_user_id INT NOT NULL, -- 提问者
    to_user_id INT NOT NULL, -- 回答者
    points_amount INT NOT NULL, -- 积分数量
    transaction_type INT NOT NULL, -- 1:悬赏发布 2:答案采纳 3:悬赏退回
    transaction_time DATE DEFAULT CURRENT_DATE,
    description VARCHAR(256),
    FOREIGN KEY (question_id) REFERENCES QUESTION_REWARD(QUESTION_ID) ON DELETE CASCADE,
    FOREIGN KEY (from_user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE,
    FOREIGN KEY (to_user_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE
);

-- 问题关注表（用户关注感兴趣的问题）
CREATE TABLE QUESTION_FOLLOW (
    follow_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    question_id INT NOT NULL,
    follower_id INT NOT NULL,
    follow_time DATE DEFAULT CURRENT_DATE,
    notification_enabled NUMBER(1) DEFAULT 1, -- 是否接收通知
    FOREIGN KEY (question_id) REFERENCES QUESTION_REWARD(QUESTION_ID) ON DELETE CASCADE,
    FOREIGN KEY (follower_id) REFERENCES "USER"(USER_ID) ON DELETE CASCADE,
    UNIQUE(question_id, follower_id)
);

-- 创建索引
CREATE INDEX idx_question_reward_status ON QUESTION_REWARD(STATUS, CREATED_TIME DESC);
CREATE INDEX idx_question_reward_points ON QUESTION_REWARD(REWARD_POINTS DESC);
CREATE INDEX idx_question_reward_questioner ON QUESTION_REWARD(QUESTIONER_ID);
CREATE INDEX idx_question_answer_question ON QUESTION_ANSWER(QUESTION_ID, CREATED_TIME);
CREATE INDEX idx_question_answer_answerer ON QUESTION_ANSWER(ANSWERER_ID);
CREATE INDEX idx_question_answer_best ON QUESTION_ANSWER(IS_BEST_ANSWER, HELPFUL_COUNT DESC);
CREATE INDEX idx_answer_evaluation_answer ON ANSWER_EVALUATION(ANSWER_ID);
CREATE INDEX idx_reward_transaction_question ON REWARD_TRANSACTION(QUESTION_ID);

-- 添加表注释
COMMENT ON TABLE QUESTION_REWARD IS '悬赏问题表';
COMMENT ON COLUMN QUESTION_REWARD.reward_points IS '悬赏积分数量';
COMMENT ON COLUMN QUESTION_REWARD.status IS '状态：1进行中 2已解决 3已截止 4已取消';
COMMENT ON COLUMN QUESTION_REWARD.best_answer_id IS '最佳答案ID';

COMMENT ON TABLE QUESTION_ANSWER IS '问题回答表';
COMMENT ON COLUMN QUESTION_ANSWER.is_best_answer IS '是否为最佳答案';
COMMENT ON COLUMN QUESTION_ANSWER.helpful_count IS '有用投票数';

COMMENT ON TABLE ANSWER_EVALUATION IS '答案评价表';
COMMENT ON COLUMN ANSWER_EVALUATION.evaluation_type IS '评价类型：1有用 0无用';

COMMENT ON TABLE REWARD_TRANSACTION IS '悬赏积分交易记录表';
COMMENT ON COLUMN REWARD_TRANSACTION.transaction_type IS '交易类型：1悬赏发布 2答案采纳 3悬赏退回';

-- 创建触发器：更新问题统计
CREATE OR REPLACE TRIGGER trg_update_question_stats
AFTER INSERT ON QUESTION_ANSWER
FOR EACH ROW
BEGIN
    -- 更新问题的回答数量
    UPDATE QUESTION_REWARD 
    SET answer_count = answer_count + 1
    WHERE question_id = :NEW.question_id;
END;
/

-- 创建触发器：采纳最佳答案时的处理
CREATE OR REPLACE TRIGGER trg_adopt_best_answer
AFTER UPDATE OF IS_BEST_ANSWER ON QUESTION_ANSWER
FOR EACH ROW
WHEN (NEW.IS_BEST_ANSWER = 1 AND OLD.IS_BEST_ANSWER = 0)
BEGIN
    -- 更新问题状态为已解决
    UPDATE QUESTION_REWARD 
    SET status = 2,
        best_answer_id = :NEW.answer_id,
        resolved_time = CURRENT_DATE
    WHERE question_id = :NEW.question_id;
    
    -- 记录积分交易
    INSERT INTO REWARD_TRANSACTION (
        question_id, from_user_id, to_user_id, points_amount, 
        transaction_type, description
    )
    SELECT 
        :NEW.question_id,
        qr.questioner_id,
        :NEW.answerer_id,
        qr.reward_points,
        2,
        '最佳答案悬赏'
    FROM QUESTION_REWARD qr 
    WHERE qr.question_id = :NEW.question_id;
    
    -- 更新回答者获得的悬赏积分
    UPDATE QUESTION_ANSWER 
    SET reward_received = (
        SELECT reward_points 
        FROM QUESTION_REWARD 
        WHERE question_id = :NEW.question_id
    )
    WHERE answer_id = :NEW.answer_id;
END;
/

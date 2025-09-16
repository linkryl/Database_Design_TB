-- 搜索功能相关索引和优化
-- TreeHole论坛搜索功能增强

-- 为帖子表添加全文搜索索引
CREATE INDEX idx_post_title_content ON POST(TITLE, CONTENT);
CREATE INDEX idx_post_creation_date_desc ON POST(CREATION_DATE DESC);
CREATE INDEX idx_post_like_count_desc ON POST(LIKE_COUNT DESC);
CREATE INDEX idx_post_category_creation ON POST(CATEGORY_ID, CREATION_DATE DESC);

-- 为用户表添加搜索索引
CREATE INDEX idx_user_name ON "USER"(USER_NAME);
CREATE INDEX idx_user_status_active ON "USER"(STATUS) WHERE STATUS = 1;

-- 为贴吧表添加搜索索引
CREATE INDEX idx_bar_name ON BAR(BAR_NAME);
CREATE INDEX idx_bar_status_active ON BAR(STATUS) WHERE STATUS = 1;

-- 搜索历史表
CREATE TABLE SEARCH_HISTORY (
    search_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id INT,
    search_keyword VARCHAR(256) NOT NULL,
    search_type INT NOT NULL, -- 1:帖子 2:用户 3:贴吧 4:综合
    result_count INT DEFAULT 0,
    search_time DATE DEFAULT CURRENT_DATE,
    ip_address VARCHAR(45),
    FOREIGN KEY (user_id) REFERENCES "USER"(USER_ID) ON DELETE SET NULL
);

-- 热门搜索词表
CREATE TABLE HOT_SEARCH_KEYWORDS (
    keyword_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    keyword VARCHAR(256) NOT NULL UNIQUE,
    search_count INT DEFAULT 1,
    last_search_time DATE DEFAULT CURRENT_DATE,
    created_time DATE DEFAULT CURRENT_DATE
);

-- 搜索建议表（用于自动补全）
CREATE TABLE SEARCH_SUGGESTIONS (
    suggestion_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    keyword VARCHAR(256) NOT NULL,
    suggestion VARCHAR(256) NOT NULL,
    weight INT DEFAULT 1, -- 权重，用于排序
    is_active INT DEFAULT 1,
    created_time DATE DEFAULT CURRENT_DATE
);

-- 为搜索相关表添加注释
COMMENT ON TABLE SEARCH_HISTORY IS '用户搜索历史记录';
COMMENT ON COLUMN SEARCH_HISTORY.search_id IS '搜索记录ID';
COMMENT ON COLUMN SEARCH_HISTORY.user_id IS '用户ID，可为空（游客搜索）';
COMMENT ON COLUMN SEARCH_HISTORY.search_keyword IS '搜索关键词';
COMMENT ON COLUMN SEARCH_HISTORY.search_type IS '搜索类型：1帖子 2用户 3贴吧 4综合';
COMMENT ON COLUMN SEARCH_HISTORY.result_count IS '搜索结果数量';
COMMENT ON COLUMN SEARCH_HISTORY.search_time IS '搜索时间';

COMMENT ON TABLE HOT_SEARCH_KEYWORDS IS '热门搜索关键词';
COMMENT ON COLUMN HOT_SEARCH_KEYWORDS.keyword IS '搜索关键词';
COMMENT ON COLUMN HOT_SEARCH_KEYWORDS.search_count IS '搜索次数';
COMMENT ON COLUMN HOT_SEARCH_KEYWORDS.last_search_time IS '最后搜索时间';

COMMENT ON TABLE SEARCH_SUGGESTIONS IS '搜索建议词表';
COMMENT ON COLUMN SEARCH_SUGGESTIONS.keyword IS '输入关键词';
COMMENT ON COLUMN SEARCH_SUGGESTIONS.suggestion IS '建议词';
COMMENT ON COLUMN SEARCH_SUGGESTIONS.weight IS '权重，用于排序';

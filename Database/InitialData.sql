-- 帖子分类初始数据
-- 为TreeHole系统添加默认的帖子分类

-- 插入默认帖子分类
INSERT INTO POST_CATEGORY (category_id, category) VALUES (1, '生活分享');
INSERT INTO POST_CATEGORY (category_id, category) VALUES (2, '学习交流');
INSERT INTO POST_CATEGORY (category_id, category) VALUES (3, '情感树洞');
INSERT INTO POST_CATEGORY (category_id, category) VALUES (4, '校园资讯');
INSERT INTO POST_CATEGORY (category_id, category) VALUES (5, '兴趣爱好');
INSERT INTO POST_CATEGORY (category_id, category) VALUES (6, '求助问答');
INSERT INTO POST_CATEGORY (category_id, category) VALUES (7, '其他');

-- 提交事务
COMMIT;

-- 向用户打卡表插入记录：增加经验点数
CREATE OR REPLACE TRIGGER increment_experience_points
AFTER INSERT ON USER_CHECK_IN
FOR EACH ROW
BEGIN
    UPDATE "USER"
    SET experience_points = experience_points + 100
    WHERE user_id = :NEW.user_id;
END;

-- 从用户打卡表删除记录：减少经验点数
CREATE OR REPLACE TRIGGER decrement_experience_points
AFTER DELETE ON USER_CHECK_IN
FOR EACH ROW
BEGIN
    UPDATE "USER"
    SET experience_points = experience_points - 100
    WHERE user_id = :OLD.user_id;
END;

-- 向用户关注表插入记录：增加关注数和被关注数
CREATE OR REPLACE TRIGGER increment_follow_counts
AFTER INSERT ON USER_FOLLOW
FOR EACH ROW
BEGIN
    UPDATE "USER"
    SET follows_count = follows_count + 1
    WHERE user_id = :NEW.follower_id;
    UPDATE "USER"
    SET followed_count = followed_count + 1
    WHERE user_id = :NEW.user_id;
END;

-- 从用户关注表删除记录：减少关注数和被关注数
CREATE OR REPLACE TRIGGER decrement_follow_counts
AFTER DELETE ON USER_FOLLOW
FOR EACH ROW
BEGIN
    UPDATE "USER"
    SET follows_count = follows_count - 1
    WHERE user_id = :OLD.follower_id;
    UPDATE "USER"
    SET followed_count = followed_count - 1
    WHERE user_id = :OLD.user_id;
END;

-- 向帖子评论表插入记录：增加评论数
CREATE OR REPLACE TRIGGER increment_post_c_count
AFTER INSERT ON POST_COMMENT
FOR EACH ROW
BEGIN
    UPDATE POST
    SET comment_count = comment_count + 1
    WHERE post_id = :NEW.post_id;
END;

-- 从帖子评论表删除记录：减少评论数
CREATE OR REPLACE TRIGGER decrement_post_c_count
AFTER DELETE ON POST_COMMENT
FOR EACH ROW
BEGIN
    UPDATE POST
    SET comment_count = comment_count - 1
    WHERE post_id = :OLD.post_id;
END;

-- 向帖子点赞表插入记录：增加点赞数和被点赞数
CREATE OR REPLACE TRIGGER increment_post_like_counts
AFTER INSERT ON POST_LIKE
FOR EACH ROW
DECLARE
    poster_user_id INT;
BEGIN
    -- 先获取poster_user_id，避免在UPDATE后读取POST表
    SELECT user_id INTO poster_user_id
    FROM POST
    WHERE post_id = :NEW.post_id;
    
    -- 然后更新POST表
    UPDATE POST
    SET like_count = like_count + 1
    WHERE post_id = :NEW.post_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET liked_count = liked_count + 1
    WHERE user_id = poster_user_id;
    UPDATE "USER"
    SET likes_count = likes_count + 1
    WHERE user_id = :NEW.user_id;
END;

-- 从帖子点赞表删除记录：减少点赞数和被点赞数
CREATE OR REPLACE TRIGGER decrement_post_like_counts
AFTER DELETE ON POST_LIKE
FOR EACH ROW
DECLARE
    poster_user_id INT;
BEGIN
    -- 先获取poster_user_id，避免在UPDATE后读取POST表
    SELECT user_id INTO poster_user_id
    FROM POST
    WHERE post_id = :OLD.post_id;
    
    -- 然后更新POST表
    UPDATE POST
    SET like_count = like_count - 1
    WHERE post_id = :OLD.post_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET liked_count = liked_count - 1
    WHERE user_id = poster_user_id;
    UPDATE "USER"
    SET likes_count = likes_count - 1
    WHERE user_id = :OLD.user_id;
END;

-- 向帖子点踩表插入记录：增加点踩数
CREATE OR REPLACE TRIGGER increment_post_dislike_count
AFTER INSERT ON POST_DISLIKE
FOR EACH ROW
BEGIN
    UPDATE POST
    SET dislike_count = dislike_count + 1
    WHERE post_id = :NEW.post_id;
END;

-- 从帖子点踩表删除记录：减少点踩数
CREATE OR REPLACE TRIGGER decrement_post_dislike_count
AFTER DELETE ON POST_DISLIKE
FOR EACH ROW
BEGIN
    UPDATE POST
    SET dislike_count = dislike_count - 1
    WHERE post_id = :OLD.post_id;
END;

-- 向帖子评论点赞表插入记录：增加点赞数和被点赞数
CREATE OR REPLACE TRIGGER increment_post_c_like_counts
AFTER INSERT ON POST_COMMENT_LIKE
FOR EACH ROW
DECLARE
    commenter_user_id INT;
BEGIN
    -- 先获取commenter_user_id，避免在UPDATE后读取POST_COMMENT表
    SELECT user_id INTO commenter_user_id
    FROM POST_COMMENT
    WHERE comment_id = :NEW.comment_id;
    
    -- 然后更新POST_COMMENT表
    UPDATE POST_COMMENT
    SET like_count = like_count + 1
    WHERE comment_id = :NEW.comment_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET liked_count = liked_count + 1
    WHERE user_id = commenter_user_id;
    UPDATE "USER"
    SET likes_count = likes_count + 1
    WHERE user_id = :NEW.user_id;
END;

-- 从帖子评论点赞表删除记录：减少点赞数和被点赞数
CREATE OR REPLACE TRIGGER decrement_post_c_like_counts
AFTER DELETE ON POST_COMMENT_LIKE
FOR EACH ROW
DECLARE
    commenter_user_id INT;
BEGIN
    -- 先获取commenter_user_id，避免在UPDATE后读取POST_COMMENT表
    SELECT user_id INTO commenter_user_id
    FROM POST_COMMENT
    WHERE comment_id = :OLD.comment_id;
    
    -- 然后更新POST_COMMENT表
    UPDATE POST_COMMENT
    SET like_count = like_count - 1
    WHERE comment_id = :OLD.comment_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET liked_count = liked_count - 1
    WHERE user_id = commenter_user_id;
    UPDATE "USER"
    SET likes_count = likes_count - 1
    WHERE user_id = :OLD.user_id;
END;

-- 向帖子评论点踩表插入记录：增加点踩数
CREATE OR REPLACE TRIGGER increment_post_c_dislike_count
AFTER INSERT ON POST_COMMENT_DISLIKE
FOR EACH ROW
BEGIN
    UPDATE POST_COMMENT
    SET dislike_count = dislike_count + 1
    WHERE comment_id = :NEW.comment_id;
END;

-- 从帖子评论点踩表删除记录：减少点踩数
CREATE OR REPLACE TRIGGER decrement_post_c_dislike_count
AFTER DELETE ON POST_COMMENT_DISLIKE
FOR EACH ROW
BEGIN
    UPDATE POST_COMMENT
    SET dislike_count = dislike_count - 1
    WHERE comment_id = :OLD.comment_id;
END;

-- 向帖子收藏表插入记录：增加收藏数和被收藏数
CREATE OR REPLACE TRIGGER increment_post_favorite_counts
AFTER INSERT ON POST_FAVORITE
FOR EACH ROW
DECLARE
    poster_user_id INT;
BEGIN
    -- 先获取poster_user_id，避免在UPDATE后读取POST表
    SELECT user_id INTO poster_user_id
    FROM POST
    WHERE post_id = :NEW.post_id;
    
    -- 然后更新POST表
    UPDATE POST
    SET favorite_count = favorite_count + 1
    WHERE post_id = :NEW.post_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET favorited_count = favorited_count + 1
    WHERE user_id = poster_user_id;
    UPDATE "USER"
    SET favorites_count = favorites_count + 1
    WHERE user_id = :NEW.user_id;
END;

-- 从帖子收藏表删除记录：减少收藏数和被收藏数
CREATE OR REPLACE TRIGGER decrement_post_favorite_counts
AFTER DELETE ON POST_FAVORITE
FOR EACH ROW
DECLARE
    poster_user_id INT;
BEGIN
    -- 先获取poster_user_id，避免在UPDATE后读取POST表
    SELECT user_id INTO poster_user_id
    FROM POST
    WHERE post_id = :OLD.post_id;
    
    -- 然后更新POST表
    UPDATE POST
    SET favorite_count = favorite_count - 1
    WHERE post_id = :OLD.post_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET favorited_count = favorited_count - 1
    WHERE user_id = poster_user_id;
    UPDATE "USER"
    SET favorites_count = favorites_count - 1
    WHERE user_id = :OLD.user_id;
END;

-- 向新闻评论表插入记录：增加评论数
CREATE OR REPLACE TRIGGER increment_news_c_count
AFTER INSERT ON NEWS_COMMENT
FOR EACH ROW
BEGIN
    UPDATE NEWS
    SET comment_count = comment_count + 1
    WHERE news_id = :NEW.news_id;
END;

-- 从新闻评论表删除记录：减少评论数
CREATE OR REPLACE TRIGGER decrement_news_c_count
AFTER DELETE ON NEWS_COMMENT
FOR EACH ROW
BEGIN
    UPDATE NEWS
    SET comment_count = comment_count - 1
    WHERE news_id = :OLD.news_id;
END;

-- 向新闻点赞表插入记录：增加点赞数和被点赞数
CREATE OR REPLACE TRIGGER increment_news_like_counts
AFTER INSERT ON NEWS_LIKE
FOR EACH ROW
DECLARE
    news_owner_id INT;
BEGIN
    -- 先获取news_owner_id，避免在UPDATE后读取NEWS表
    SELECT user_id INTO news_owner_id
    FROM NEWS
    WHERE news_id = :NEW.news_id;
    
    -- 然后更新NEWS表
    UPDATE NEWS
    SET like_count = like_count + 1
    WHERE news_id = :NEW.news_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET liked_count = liked_count + 1
    WHERE user_id = news_owner_id;
    UPDATE "USER"
    SET likes_count = likes_count + 1
    WHERE user_id = :NEW.user_id;
END;

-- 从新闻点赞表删除记录：减少点赞数和被点赞数
CREATE OR REPLACE TRIGGER decrement_news_like_counts
AFTER DELETE ON NEWS_LIKE
FOR EACH ROW
DECLARE
    news_owner_id INT;
BEGIN
    -- 先获取news_owner_id，避免在UPDATE后读取NEWS表
    SELECT user_id INTO news_owner_id
    FROM NEWS
    WHERE news_id = :OLD.news_id;
    
    -- 然后更新NEWS表
    UPDATE NEWS
    SET like_count = like_count - 1
    WHERE news_id = :OLD.news_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET liked_count = liked_count - 1
    WHERE user_id = news_owner_id;
    UPDATE "USER"
    SET likes_count = likes_count - 1
    WHERE user_id = :OLD.user_id;
END;

-- 向新闻点踩表插入记录：增加点踩数
CREATE OR REPLACE TRIGGER increment_news_dislike_count
AFTER INSERT ON NEWS_DISLIKE
FOR EACH ROW
BEGIN
    UPDATE NEWS
    SET dislike_count = dislike_count + 1
    WHERE news_id = :NEW.news_id;
END;

-- 从新闻点踩表删除记录：减少点踩数
CREATE OR REPLACE TRIGGER decrement_news_dislike_count
AFTER DELETE ON NEWS_DISLIKE
FOR EACH ROW
BEGIN
    UPDATE NEWS
    SET dislike_count = dislike_count - 1
    WHERE news_id = :OLD.news_id;
END;

-- 向新闻评论点赞表插入记录：增加点赞数和被点赞数
CREATE OR REPLACE TRIGGER increment_news_c_like_counts
AFTER INSERT ON NEWS_COMMENT_LIKE
FOR EACH ROW
DECLARE
    comment_user_id INT;
BEGIN
    -- 先获取comment_user_id，避免在UPDATE后读取NEWS_COMMENT表
    SELECT user_id INTO comment_user_id
    FROM NEWS_COMMENT
    WHERE comment_id = :NEW.comment_id;
    
    -- 然后更新NEWS_COMMENT表
    UPDATE NEWS_COMMENT
    SET like_count = like_count + 1
    WHERE comment_id = :NEW.comment_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET liked_count = liked_count + 1
    WHERE user_id = comment_user_id;
    UPDATE "USER"
    SET likes_count = likes_count + 1
    WHERE user_id = :NEW.user_id;
END;

-- 从新闻评论点赞表删除记录：减少点赞数和被点赞数
CREATE OR REPLACE TRIGGER decrement_news_c_like_counts
AFTER DELETE ON NEWS_COMMENT_LIKE
FOR EACH ROW
DECLARE
    comment_user_id INT;
BEGIN
    -- 先获取comment_user_id，避免在UPDATE后读取NEWS_COMMENT表
    SELECT user_id INTO comment_user_id
    FROM NEWS_COMMENT
    WHERE comment_id = :OLD.comment_id;
    
    -- 然后更新NEWS_COMMENT表
    UPDATE NEWS_COMMENT
    SET like_count = like_count - 1
    WHERE comment_id = :OLD.comment_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET liked_count = liked_count - 1
    WHERE user_id = comment_user_id;
    UPDATE "USER"
    SET likes_count = likes_count - 1
    WHERE user_id = :OLD.user_id;
END;

-- 向新闻评论点踩表插入记录：增加点踩数
CREATE OR REPLACE TRIGGER increment_news_c_dislike_count
AFTER INSERT ON NEWS_COMMENT_DISLIKE
FOR EACH ROW
BEGIN
    UPDATE NEWS_COMMENT
    SET dislike_count = dislike_count + 1
    WHERE comment_id = :NEW.comment_id;
END;

-- 从新闻评论点踩表删除记录：减少点踩数
CREATE OR REPLACE TRIGGER decrement_news_c_dislike_count
AFTER DELETE ON NEWS_COMMENT_DISLIKE
FOR EACH ROW
BEGIN
    UPDATE NEWS_COMMENT
    SET dislike_count = dislike_count - 1
    WHERE comment_id = :OLD.comment_id;
END;

-- 向新闻收藏表插入记录：增加收藏数和被收藏数
CREATE OR REPLACE TRIGGER increment_news_favorite_counts
AFTER INSERT ON NEWS_FAVORITE
FOR EACH ROW
DECLARE
    news_owner_id INT;
BEGIN
    -- 先获取news_owner_id，避免在UPDATE后读取NEWS表
    SELECT user_id INTO news_owner_id
    FROM NEWS
    WHERE news_id = :NEW.news_id;
    
    -- 然后更新NEWS表
    UPDATE NEWS
    SET favorite_count = favorite_count + 1
    WHERE news_id = :NEW.news_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET favorited_count = favorited_count + 1
    WHERE user_id = news_owner_id;
    UPDATE "USER"
    SET favorites_count = favorites_count + 1
    WHERE user_id = :NEW.user_id;
END;

-- 从新闻收藏表删除记录：减少收藏数和被收藏数
CREATE OR REPLACE TRIGGER decrement_news_favorite_counts
AFTER DELETE ON NEWS_FAVORITE
FOR EACH ROW
DECLARE
    news_owner_id INT;
BEGIN
    -- 先获取news_owner_id，避免在UPDATE后读取NEWS表
    SELECT user_id INTO news_owner_id
    FROM NEWS
    WHERE news_id = :OLD.news_id;
    
    -- 然后更新NEWS表
    UPDATE NEWS
    SET favorite_count = favorite_count - 1
    WHERE news_id = :OLD.news_id;
    
    -- 更新用户统计
    UPDATE "USER"
    SET favorited_count = favorited_count - 1
    WHERE user_id = news_owner_id;
    UPDATE "USER"
    SET favorites_count = favorites_count - 1
    WHERE user_id = :OLD.user_id;
END;

-- 用户注册时向用户设置表插入记录
CREATE OR REPLACE TRIGGER add_user_setting_after_insert
AFTER INSERT ON "USER"
FOR EACH ROW
BEGIN
    INSERT INTO USER_SETTING (
        user_id,
        is_telephone_public,
        is_registration_date_public,
        is_profile_public,
        is_gender_public,
        is_birthdate_public,
        is_level_public,
        is_following_count_public,
        is_followers_count_public,
        is_favorites_count_public,
        is_favored_count_public,
        is_likes_count_public,
        is_liked_count_public,
        is_message_count_public,
        allow_following,
        receive_admin_notifications,
        receive_user_notifications
    ) VALUES (
        :NEW.user_id,
        0,  -- is_telephone_public
        1,  -- is_registration_date_public
        1,  -- is_profile_public
        1,  -- is_gender_public
        0,  -- is_birthdate_public
        1,  -- is_level_public
        1,  -- is_following_count_public
        1,  -- is_followers_count_public
        1,  -- is_favorites_count_public
        1,  -- is_favored_count_public
        1,  -- is_likes_count_public
        1,  -- is_liked_count_public
        1,  -- is_message_count_public
        1,  -- allow_following
        1,  -- receive_admin_notifications
        1   -- receive_user_notifications
    );
END;

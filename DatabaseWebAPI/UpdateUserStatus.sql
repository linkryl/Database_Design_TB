-- 更新所有普通用户的状态为正常状态
-- 只更新角色为0（普通用户）且状态为0（封禁）的用户
UPDATE "USER" 
SET "STATUS" = 1 
WHERE "ROLE" = 0 AND "STATUS" = 0;

-- 查看更新结果
SELECT "USER_ID", "USER_NAME", "ROLE", "STATUS" 
FROM "USER" 
ORDER BY "USER_ID";

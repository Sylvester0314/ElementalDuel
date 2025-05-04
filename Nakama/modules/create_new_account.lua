local nk = require("nakama")
local misc = require("misc")

local function get_current_uid()
    local storage_data = nk.storage_read({
        { collection = "global", key = "uid_counter"} }
    )

    if #storage_data == 0 then
        return 10000
    else
        return storage_data[1].value.uid
    end
end

local function update_uid_counter(uid)
    local value = { uid = uid }
    local result = nk.storage_write({
        { collection = "global", key = "uid_counter", value = value }
    })
    return result
end

local function generate_uid()
    local current_uid = get_current_uid()
    local new_uid = tostring(current_uid + 1)

    update_uid_counter(new_uid)

    return new_uid
end

local function check_device_registerd(device_id)
    local query = "SELECT * FROM user_device WHERE id = $1";
    local param = { device_id }

    return pcall(nk.sql_exec, query, param)
end

local function get_account_user_id(email)
    local query = "SELECT * FROM users WHERE email = $1"
    local param = { email }

    return pcall(nk.sql_query, query, param)
end

local function set_account_information(email, username, uid)
    local metadata = {
        uid = uid,
        score = 0,
        avatar = 1101
    }
    local query = "UPDATE users SET username = $1, metadata = $2 WHERE email = $3";
    local param = { username, metadata, email }

    return pcall(nk.sql_exec, query, param)
end

local function bind_device_id(device_id, user_id)
    local query = "INSERT INTO user_device (id, user_id) VALUES ($1, $2)"
    local param = { device_id, user_id }

    return pcall(nk.sql_exec, query, param)
end

local function create_new_account(context, payload)
    local data = nk.json_decode(payload)

    local device_id = data.device_id
    local password = data.password
    local username = data.username

    -- 检查当前设备是否已经有账号注册
    local status, result = check_device_registerd(device_id)
    if not status then
        error({result, misc.error_codes.UNKNOWN})
    elseif result ~= 0 then
        error({"account_registered", misc.error_codes.ALREADY_EXISTS})
    end

    -- 根据 uid(email格式) 和 password 注册新账号
    local uid = generate_uid()
    local email = uid .. "@weird.adachi.top"
    status, result = pcall(nk.authenticate_email, email, password)
    if not status then
        error({result, misc.error_codes.FAILED_PRECONDITION})
    end

    -- 获取新账号的 user_id
    status, result = get_account_user_id(email)
    if not status then
        error({result, misc.error_codes.UNKNOWN})
    end
    local user_id = result[1].id;

    -- 设置新账号基础信息
    status, result = set_account_information(email, username, uid)
    if not status then
        error({result, misc.error_codes.UNKNOWN})
    end

    -- 绑定设备ID
    status, result = bind_device_id(device_id, user_id)
    if not status then
        error({result, misc.error_codes.UNKNOWN})
    end

    return nk.json_encode({ uid = uid })
end

nk.register_rpc(create_new_account, "create_new_account")
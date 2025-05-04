local nk = require("nakama")

local function get_metadata(context, payload)
    local data = nk.json_decode(payload)

    local users = nk.users_get_id({ data.user_id })
    if #users == 0 then
        error({ "user_not_found" })
    end

    return nk.json_encode({
        metadata = users[1].metadata,
        username = users[1].username
    })
end


nk.register_rpc(get_metadata, "get_metadata")
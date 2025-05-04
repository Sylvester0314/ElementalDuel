local M = {}

M.error_codes = {
    OK                  = 0,  -- HTTP 200
    CANCELED            = 1,  -- HTTP 499
    UNKNOWN             = 2,  -- HTTP 500
    INVALID_ARGUMENT    = 3,  -- HTTP 400
    DEADLINE_EXCEEDED   = 4,  -- HTTP 504
    NOT_FOUND           = 5,  -- HTTP 404
    ALREADY_EXISTS      = 6,  -- HTTP 409
    PERMISSION_DENIED   = 7,  -- HTTP 403
    RESOURCE_EXHAUSTED  = 8,  -- HTTP 429
    FAILED_PRECONDITION = 9,  -- HTTP 400
    ABORTED             = 10, -- HTTP 409
    OUT_OF_RANGE        = 11, -- HTTP 400
    UNIMPLEMENTED       = 12, -- HTTP 501
    INTERNAL            = 13, -- HTTP 500
    UNAVAILABLE         = 14, -- HTTP 503
    DATA_LOSS           = 15, -- HTTP 500
    UNAUTHENTICATED     = 16  -- HTTP 401
}

function M.print_table(t, indent)
    indent = indent or 0
    local spacing = string.rep("  ", indent)

    for k, v in pairs(t) do
        if type(v) == "table" then
            print(spacing .. tostring(k) .. ":")
            M.print_table(v, indent + 1)
        else
            print(spacing .. tostring(k) .. ": " .. tostring(v))
        end
    end
end

return M
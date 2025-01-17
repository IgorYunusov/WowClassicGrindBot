﻿namespace Core
{
    public enum UI_ERROR
    {
        NONE = 0,
        ERR_BADATTACKFACING = 1,
        ERR_SPELL_FAILED_S = 2,
        ERR_SPELL_OUT_OF_RANGE = 3,
        ERR_BADATTACKPOS = 4,
        ERR_AUTOFOLLOW_TOO_FAR = 5,
        SPELL_FAILED_MOVING = 6,
        ERR_SPELL_COOLDOWN = 7,
        ERR_SPELL_FAILED_ANOTHER_IN_PROGRESS = 8,
        ERR_SPELL_FAILED_STUNNED = 9,
        ERR_SPELL_FAILED_INTERRUPTED = 10,
        SPELL_FAILED_ITEM_NOT_READY = 11,

        MAX_ERROR_RANGE = 2000,

        CAST_START = 999998,
        CAST_SUCCESS = 999999
    }
}
package com.totorowldox.REPhityOS;

import android.os.Build
import android.os.LocaleList
import com.unity3d.player.UnityPlayer
import java.util.*

object PathUtil {
    @JvmStatic
    fun getFileDir(): String {
        val ctx = UnityPlayer.currentActivity
        return ctx.filesDir.path
    }
}

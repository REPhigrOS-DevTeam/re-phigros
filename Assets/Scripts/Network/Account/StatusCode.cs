namespace Network.Account
{
    public enum StatusCode
    {
        // https://account.rephigros.top/docs/code.html
        Unknown = -1, // 出现这个可以吃席了
        OK = 100,
        InvalidParam = 101, // 一般来说都是你少传参数
        ServerInternalError = 102, // 没啥好说的，找CarlSkyCoding去
        IllegalLogin = 2011, // 没有可用登录名额时会触发
        InvalidUsername = 2012, // 用户名不存在之类的
        InvalidPassword = 2013, // 仅login时触发，登录用的密码不对劲
        InvalidToken = 2014, // Verify的时候传的token不合法
        NoPermission = 2015, // 你小子没内测权限还想登录？
        UserBanned = 2016 // 用户被封禁
    }
}

#import "UnityAppController.h"
#import "UnityInterface.h"

@interface FileSharingAppController : UnityAppController
@end

IMPL_APP_CONTROLLER_SUBCLASS(FileSharingAppController)

@implementation FileSharingAppController

- (BOOL)application:(UIApplication *)app openURL:(NSURL *)url options:(NSDictionary<NSString *, id> *)options {
    if (url) {
        NSLog(@"[File Sharing] detected file url: %@", [url absoluteString]);
            UnitySendMessage("BridgeGameObject", "Callback_GetiOSSharedFile", [[@"true\n" stringByAppendingString:[url absoluteString]] UTF8String]);
    } else {
        NSLog(@"[File Sharing] Error: url is nil");
        UnitySendMessage("BridgeGameObject", "Callback_GetiOSSharedFile", "false\n");
    }
    return [super application:app openURL:url options:options];
}

@end

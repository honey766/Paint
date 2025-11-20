// SafeAreaInsets.jslib
mergeInto(LibraryManager.library, {
    GetSafeAreaInsets: function() {
        var returnStr;
        
        // 1. 보낼 문자열 결정
        if (typeof window.TossSafeAreaInsets !== 'undefined') {
            var insets = window.TossSafeAreaInsets.get();
            // ★ 여기서 window 크기를 같이 묶어서 포장합니다!
            var payload = {
                top: insets.top,
                bottom: insets.bottom,
                left: insets.left,
                right: insets.right,
                windowWidth: window.innerWidth,
                windowHeight: window.innerHeight
            };
            returnStr = JSON.stringify(payload);
        } else {
                returnStr = JSON.stringify({
                top: 0, bottom: 0, left: 0, right: 0,
                windowWidth: window.innerWidth,
                windowHeight: window.innerHeight
            });
        }

        // 2. Unity WebGL 메모리 힙에 문자열을 위한 공간 할당 (매우 중요!)
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        
        // 3. 할당된 메모리에 문자열 복사
        stringToUTF8(returnStr, buffer, bufferSize);
        
        // 4. 메모리 포인터 반환
        return buffer;
    },
    
    SubscribeSafeArea: function(gameObjectName) {
        if (typeof window.TossSafeAreaInsets !== 'undefined') {
            var objName = UTF8ToString(gameObjectName);
            
            window.TossSafeAreaInsets.subscribe({
                onEvent: function(insets) {
                    var payload = {
                        top: insets.top,
                        bottom: insets.bottom,
                        left: insets.left,
                        right: insets.right,
                        windowWidth: window.innerWidth,
                        windowHeight: window.innerHeight
                    };
                    SendMessage(objName, 'OnSafeAreaChanged', JSON.stringify(payload));
                }
            });
        }
    }
});
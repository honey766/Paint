// ObserveVisibilityPlugin.jslib
mergeInto(LibraryManager.library, {
    StartObserveVisibility: function (goPtr, methodPtr) {
        var goName = UTF8ToString(goPtr);
        var method = UTF8ToString(methodPtr);
        
        // 이전 구독 해제
        if (typeof window.__tossVisUnsub === 'function') {
            try { window.__tossVisUnsub(); } catch (e) { console.log(e); }
            window.__tossVisUnsub = null;
        }
        
        function send(state, evt) {
            var payload = JSON.stringify({
                state: state, // "visible" | "hidden"
                eventType: evt, // "visibilitychange" | "pagehide" | "pageshow" | "blur" | "focus" | "init"
                hidden: state === 'hidden',
                ts: Date.now()
            });
            console.log(`[TOSS] ${goName} ${method} ${payload}`);
            try { SendMessage(goName, method, state === 'hidden' ? 1 : 0); } catch (e) { console.log(e); }
        }
        
        // 캡처 단계로 가장 먼저 잡는다(숨기기 직전에도 최대한 빨리 유니티 호출)
        var opts = { capture: true, passive: true };
        
        function onVisibility() { send(document.hidden ? 'hidden' : 'visible', 'visibilitychange'); }
        function onPageHide()   { send('hidden',  'pagehide'); }
        function onPageShow()   { send('visible', 'pageshow'); }
        function onBlur()       { send(document.hidden ? 'hidden' : 'visible', 'blur'); }
        function onFocus()      { send(document.hidden ? 'hidden' : 'visible', 'focus'); }
        function onFreeze()     { send('hidden', 'freeze'); }
        
        // 이벤트 구독
        document.addEventListener('visibilitychange', onVisibility, opts);
        window.addEventListener('pagehide', onPageHide, opts);
        window.addEventListener('pageshow', onPageShow, opts);
        window.addEventListener('blur', onBlur, opts);
        window.addEventListener('focus', onFocus, opts);
        window.addEventListener('freeze', onFreeze, opts);
        
        // 초기 1회 상태 통지(필요 시)
        send(document.hidden ? 'hidden' : 'visible', 'init');
        
        // 이벤트 구독 해제 함수 정의
        window.__tossVisUnsub = function () {
            document.removeEventListener('visibilitychange', onVisibility, opts);
            window.removeEventListener('pagehide', onPageHide, opts);
            window.removeEventListener('pageshow', onPageShow, opts);
            window.removeEventListener('blur', onBlur, opts);
            window.removeEventListener('focus', onFocus, opts);
            window.removeEventListener('freeze', onFreeze, opts);
        };
    }
});
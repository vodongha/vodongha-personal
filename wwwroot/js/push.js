window.pushUtils = {
    _vapidPublicKey: null,

    init(vapidPublicKey) {
        this._vapidPublicKey = vapidPublicKey;
    },

    isSupported() {
        return 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;
    },

    getPermission() {
        return Notification.permission; // 'default' | 'granted' | 'denied'
    },

    getNotificationHelpUrl() {
        const ua = navigator.userAgent;
        const isIOS           = /iPad|iPhone|iPod/.test(ua);
        const isSafari        = /^((?!chrome|android).)*safari/i.test(ua);
        const isFirefox       = /Firefox\//.test(ua);
        const isEdge          = /Edg\//.test(ua);
        const isSamsungBrowser = /SamsungBrowser\//.test(ua);
        const isOpera         = /OPR\/|Opera\//.test(ua);

        if (isIOS || isSafari)    return 'https://support.apple.com/guide/safari/customize-website-notifications-sfri40734/mac';
        if (isFirefox)            return 'https://support.mozilla.org/kb/push-notifications-firefox';
        if (isEdge)               return 'https://support.microsoft.com/microsoft-edge/manage-website-notifications-in-microsoft-edge';
        if (isSamsungBrowser)     return 'https://www.samsung.com/global/galaxy/apps/samsung-internet/';
        if (isOpera)              return 'https://help.opera.com/latest/web-preferences/';
        return 'https://support.google.com/chrome/answer/3220216';
    },

    /// Register SW + subscribe. Returns serialized subscription JSON or null on failure.
    async subscribe() {
        if (!this.isSupported() || !this._vapidPublicKey) return null;

        try {
            const reg = await navigator.serviceWorker.register('/sw.js', { scope: '/' });
            await navigator.serviceWorker.ready;

            const permission = await Notification.requestPermission();
            if (permission !== 'granted') return null;

            // ES2026: Uint8Array.fromBase64 with base64url alphabet replaces manual atob conversion
            const subscription = await reg.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: Uint8Array.fromBase64(this._vapidPublicKey, { alphabet: 'base64url' })
            });

            return JSON.stringify(subscription);
        } catch (e) {
            void e; // subscribe failed silently
            return null;
        }
    },

    /// Unsubscribe and return the endpoint so server can clean up.
    async unsubscribe() {
        if (!this.isSupported()) return null;
        try {
            const reg = await navigator.serviceWorker.getRegistration('/');
            if (!reg) return null;
            const sub = await reg.pushManager.getSubscription();
            if (!sub) return null;
            const endpoint = sub.endpoint;
            await sub.unsubscribe();
            return endpoint;
        } catch (e) {
            void e; // unsubscribe failed silently
            return null;
        }
    }
};

window.pushUtils = {
    _vapidPublicKey: null,

    // Convert VAPID public key from base64url to Uint8Array
    _urlBase64ToUint8Array: function (base64String) {
        const padding = '='.repeat((4 - base64String.length % 4) % 4);
        const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        const rawData = atob(base64);
        return Uint8Array.from([...rawData].map(c => c.charCodeAt(0)));
    },

    init: function (vapidPublicKey) {
        this._vapidPublicKey = vapidPublicKey;
    },

    isSupported: function () {
        return 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window;
    },

    getPermission: function () {
        return Notification.permission; // 'default' | 'granted' | 'denied'
    },

    /// Register SW + subscribe. Returns serialized subscription JSON or null on failure.
    subscribe: async function () {
        if (!this.isSupported() || !this._vapidPublicKey) return null;

        try {
            // Register service worker
            const reg = await navigator.serviceWorker.register('/sw.js', { scope: '/' });
            await navigator.serviceWorker.ready;

            // Request notification permission
            const permission = await Notification.requestPermission();
            if (permission !== 'granted') return null;

            // Subscribe to push
            const subscription = await reg.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: this._urlBase64ToUint8Array(this._vapidPublicKey)
            });

            return JSON.stringify(subscription);
        } catch (e) {
            console.warn('[push] subscribe failed:', e);
            return null;
        }
    },

    /// Unsubscribe and return the endpoint so server can clean up.
    unsubscribe: async function () {
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
            console.warn('[push] unsubscribe failed:', e);
            return null;
        }
    }
};

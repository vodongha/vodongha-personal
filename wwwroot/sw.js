// Service Worker for Web Push Notifications — vodongha.id.vn
self.addEventListener('push', function (event) {
    if (!event.data) return;

    let data = {};
    try { data = event.data.json(); } catch { data = { title: 'vodongha.id.vn', body: event.data.text(), url: '/' }; }

    const title = data.title || 'vodongha.id.vn';
    const options = {
        body: data.body || '',
        icon: '/favicon.png',
        badge: '/favicon.png',
        data: { url: data.url || '/' },
        requireInteraction: false,
        tag: data.url || 'default',   // group notifications by URL — replaces previous same-tag notification
        renotify: true
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    const url = event.notification.data?.url || '/';
    const isAdminUrl = url.includes('/admin/');

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then(function (clientList) {
            // Try to find an existing tab for this origin
            for (const client of clientList) {
                if (!client.url.startsWith(self.location.origin)) continue;
                if ('focus' in client) {
                    client.focus();
                    // For admin notifications: navigate to the specific session URL
                    // For visitor notifications: stay on current page and post a message to open chat
                    if (isAdminUrl) {
                        return client.navigate(url);
                    } else {
                        // Tell the page to open the chat widget
                        client.postMessage({ type: 'OPEN_CHAT' });
                        return;
                    }
                }
            }
            // No existing tab — open a new one (chat will auto-open via ?chat=open hint)
            if (clients.openWindow) {
                return clients.openWindow(isAdminUrl ? url : '/?chat=open');
            }
        })
    );
});

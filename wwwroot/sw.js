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

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then(function (clientList) {
            // If a tab for this origin is already open, focus it and navigate
            for (const client of clientList) {
                if (client.url.startsWith(self.location.origin) && 'focus' in client) {
                    client.focus();
                    return client.navigate(url);
                }
            }
            // Otherwise open a new tab
            if (clients.openWindow) {
                return clients.openWindow(url);
            }
        })
    );
});

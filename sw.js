// Power Grid HaYTooL - High Performance Service Worker
// Provides persistent client-side caching & instant repeat visits for GitHub Pages

const CACHE_NAME = 'powergrid-cache-v1.5';

const PRECACHE_ASSETS = [
  './',
  './index.html',
  './404.html',
  './style.css',
  './app.js',
  './site.webmanifest',
  './favicon.ico',
  './apple-touch-icon.png',
  './logo-42.webp',
  './logo-84.webp',
  './logo.webp',
  './logo.png',
  './mascot-230.webp',
  './mascot-230.png',
  './mascot.webp',
  './mascot.png',
  './mascot_404-175.webp',
  './mascot_404.webp',
  './mascot_404.png',
  './screenshots/main_window.webp',
  './screenshots/main_window.png',
  './screenshots/smart_gpu_switch.webp',
  './screenshots/smart_gpu_switch.png',
  './screenshots/plan_compare.webp',
  './screenshots/plan_compare.png'
];

// 1. Install Event: Pre-cache core static assets
self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME).then((cache) => {
      return cache.addAll(PRECACHE_ASSETS);
    }).then(() => self.skipWaiting())
  );
});

// 2. Activate Event: Clean up old caches
self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) => {
      return Promise.all(
        keys.map((key) => {
          if (key !== CACHE_NAME) {
            return caches.delete(key);
          }
        })
      );
    }).then(() => self.clients.claim())
  );
});

// 3. Fetch Event: Stale-While-Revalidate for HTML / Navigation, Cache-First for static assets
self.addEventListener('fetch', (event) => {
  const req = event.request;
  const url = new URL(req.url);

  // Only handle GET requests within same origin or Google Fonts
  if (req.method !== 'GET') return;

  // Stale-While-Revalidate Strategy for HTML pages
  if (req.mode === 'navigate' || req.headers.get('accept')?.includes('text/html')) {
    event.respondWith(
      caches.open(CACHE_NAME).then(async (cache) => {
        const cachedResponse = await cache.match(req);
        const fetchPromise = fetch(req).then((networkResponse) => {
          if (networkResponse && networkResponse.status === 200) {
            cache.put(req, networkResponse.clone());
          }
          return networkResponse;
        }).catch(() => cachedResponse);

        return cachedResponse || fetchPromise;
      })
    );
    return;
  }

  // Cache-First for static assets (images, css, js, fonts)
  event.respondWith(
    caches.match(req).then((cachedResponse) => {
      if (cachedResponse) {
        return cachedResponse;
      }
      return fetch(req).then((networkResponse) => {
        if (!networkResponse || networkResponse.status !== 200 || networkResponse.type !== 'basic' && !req.url.includes('fonts.g')) {
          return networkResponse;
        }
        const responseToCache = networkResponse.clone();
        caches.open(CACHE_NAME).then((cache) => {
          cache.put(req, responseToCache);
        });
        return networkResponse;
      });
    })
  );
});

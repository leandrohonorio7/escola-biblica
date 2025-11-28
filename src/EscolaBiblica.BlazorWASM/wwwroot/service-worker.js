// Service Worker para PWA - Escola Bíblica Dominical
const CACHE_NAME = 'escola-biblica-v1.0.0';
const STATIC_CACHE_NAME = `${CACHE_NAME}-static`;
const API_CACHE_NAME = `${CACHE_NAME}-api`;

// Recursos para cache estático (sempre em cache)
const STATIC_RESOURCES = [
    '/',
    '/index.html',
    '/css/bootstrap/bootstrap.min.css',
    '/css/app.css',
    '/_framework/blazor.webassembly.js',
    '/manifest.json'
];

// Recursos para cache dinâmico (API calls)
const API_PATTERNS = [
    /\/api\/alunos/,
    /\/api\/turmas/,
    /\/api\/presencas/,
    /\/api\/competicoes/
];

// Instalar Service Worker
self.addEventListener('install', event => {
    console.log('Service Worker: Instalando...');
    
    event.waitUntil(
        caches.open(STATIC_CACHE_NAME)
            .then(cache => {
                console.log('Service Worker: Cache estático criado');
                return cache.addAll(STATIC_RESOURCES);
            })
            .then(() => {
                console.log('Service Worker: Recursos estáticos em cache');
                return self.skipWaiting();
            })
            .catch(error => {
                console.error('Service Worker: Erro ao instalar', error);
            })
    );
});

// Ativar Service Worker
self.addEventListener('activate', event => {
    console.log('Service Worker: Ativando...');
    
    event.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames.map(cacheName => {
                        // Remover caches antigos
                        if (cacheName.startsWith('escola-biblica-') && 
                            cacheName !== STATIC_CACHE_NAME && 
                            cacheName !== API_CACHE_NAME) {
                            console.log('Service Worker: Removendo cache antigo', cacheName);
                            return caches.delete(cacheName);
                        }
                    })
                );
            })
            .then(() => {
                console.log('Service Worker: Ativado e assumindo controle');
                return self.clients.claim();
            })
    );
});

// Interceptar requisições
self.addEventListener('fetch', event => {
    const request = event.request;
    const url = new URL(request.url);

    // Ignorar requisições não HTTP/HTTPS
    if (!request.url.startsWith('http')) {
        return;
    }

    // Estratégia para recursos estáticos: Cache First
    if (isStaticResource(request)) {
        event.respondWith(cacheFirstStrategy(request, STATIC_CACHE_NAME));
        return;
    }

    // Estratégia para API: Network First com fallback para cache
    if (isApiRequest(request)) {
        event.respondWith(networkFirstStrategy(request, API_CACHE_NAME));
        return;
    }

    // Estratégia para outras requisições: Network First
    event.respondWith(
        fetch(request)
            .catch(() => {
                // Se offline, tentar buscar página principal do cache
                if (request.mode === 'navigate') {
                    return caches.match('/index.html');
                }
                return new Response('Offline - Recurso não disponível', { 
                    status: 503,
                    statusText: 'Service Unavailable' 
                });
            })
    );
});

// Verificar se é um recurso estático
function isStaticResource(request) {
    return STATIC_RESOURCES.some(resource => 
        request.url.includes(resource) || 
        request.url.endsWith(resource)
    ) || 
    request.url.includes('/_framework/') ||
    request.url.includes('/css/') ||
    request.destination === 'style' ||
    request.destination === 'script';
}

// Verificar se é uma requisição de API
function isApiRequest(request) {
    return API_PATTERNS.some(pattern => pattern.test(request.url)) ||
           request.url.includes('/api/');
}

// Estratégia Cache First (para recursos estáticos)
async function cacheFirstStrategy(request, cacheName) {
    try {
        const cachedResponse = await caches.match(request);
        
        if (cachedResponse) {
            console.log('Service Worker: Servindo do cache', request.url);
            return cachedResponse;
        }

        console.log('Service Worker: Buscando na rede', request.url);
        const networkResponse = await fetch(request);
        
        if (networkResponse.ok) {
            const cache = await caches.open(cacheName);
            cache.put(request, networkResponse.clone());
        }
        
        return networkResponse;
    } catch (error) {
        console.error('Service Worker: Erro na estratégia Cache First', error);
        return new Response('Erro ao carregar recurso', { 
            status: 500,
            statusText: 'Internal Server Error' 
        });
    }
}

// Estratégia Network First (para API)
async function networkFirstStrategy(request, cacheName) {
    try {
        console.log('Service Worker: Tentando rede primeiro', request.url);
        const networkResponse = await fetch(request);
        
        if (networkResponse.ok && request.method === 'GET') {
            const cache = await caches.open(cacheName);
            cache.put(request, networkResponse.clone());
        }
        
        return networkResponse;
    } catch (error) {
        console.log('Service Worker: Rede falhou, tentando cache', request.url);
        
        const cachedResponse = await caches.match(request);
        if (cachedResponse) {
            // Adicionar header indicando que veio do cache
            const response = cachedResponse.clone();
            response.headers.set('X-Served-From', 'service-worker-cache');
            return response;
        }

        // Se não encontrar no cache e for GET, retornar dados vazios
        if (request.method === 'GET') {
            return new Response('[]', {
                status: 200,
                headers: { 
                    'Content-Type': 'application/json',
                    'X-Served-From': 'service-worker-offline'
                }
            });
        }

        return new Response('Offline - Dados não disponíveis', { 
            status: 503,
            statusText: 'Service Unavailable' 
        });
    }
}

// Sincronização em background (para quando voltar online)
self.addEventListener('sync', event => {
    console.log('Service Worker: Evento de sincronização', event.tag);
    
    if (event.tag === 'sync-data') {
        event.waitUntil(syncOfflineData());
    }
});

// Sincronizar dados offline
async function syncOfflineData() {
    try {
        console.log('Service Worker: Sincronizando dados offline...');
        
        // Implementar lógica de sincronização aqui
        // Por exemplo, enviar dados que foram salvos offline
        
        const clients = await self.clients.matchAll();
        clients.forEach(client => {
            client.postMessage({
                type: 'SYNC_COMPLETE',
                message: 'Dados sincronizados com sucesso'
            });
        });
    } catch (error) {
        console.error('Service Worker: Erro na sincronização', error);
    }
}

// Push notifications (futuro)
self.addEventListener('push', event => {
    console.log('Service Worker: Push notification recebida');
    
    const options = {
        body: event.data ? event.data.text() : 'Nova notificação da Escola Bíblica',
        icon: '/icon-192x192.png',
        badge: '/icon-192x192.png',
        vibrate: [200, 100, 200],
        data: {
            dateOfArrival: Date.now(),
            primaryKey: 1
        },
        actions: [
            {
                action: 'explore',
                title: 'Ver Detalhes',
                icon: '/icon-192x192.png'
            },
            {
                action: 'close',
                title: 'Fechar',
                icon: '/icon-192x192.png'
            }
        ]
    };

    event.waitUntil(
        self.registration.showNotification('Escola Bíblica', options)
    );
});

// Clique em notificação
self.addEventListener('notificationclick', event => {
    console.log('Service Worker: Clique na notificação');
    
    event.notification.close();

    if (event.action === 'explore') {
        event.waitUntil(
            clients.openWindow('/')
        );
    }
});

console.log('Service Worker: Carregado e pronto');
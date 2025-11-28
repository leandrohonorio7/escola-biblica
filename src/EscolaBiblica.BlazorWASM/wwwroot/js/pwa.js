// Registro e gerenciamento do Service Worker
window.blazorPWA = {
    // Registrar Service Worker
    registerServiceWorker: function() {
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/service-worker.js')
                .then(registration => {
                    console.log('PWA: Service Worker registrado com sucesso', registration.scope);
                    
                    // Verificar atualizações
                    registration.addEventListener('updatefound', () => {
                        const newWorker = registration.installing;
                        newWorker.addEventListener('statechange', () => {
                            if (newWorker.state === 'installed' && navigator.serviceWorker.controller) {
                                console.log('PWA: Nova versão disponível');
                                this.showUpdateAvailable();
                            }
                        });
                    });
                })
                .catch(error => {
                    console.error('PWA: Erro ao registrar Service Worker', error);
                });

            // Escutar mensagens do Service Worker
            navigator.serviceWorker.addEventListener('message', event => {
                console.log('PWA: Mensagem do Service Worker', event.data);
                
                if (event.data.type === 'SYNC_COMPLETE') {
                    this.showSyncComplete(event.data.message);
                }
            });
        }
    },

    // Mostrar notificação de atualização disponível
    showUpdateAvailable: function() {
        if (window.blazorPWAComponent) {
            window.blazorPWAComponent.invokeMethodAsync('ShowUpdateAvailable');
        }
    },

    // Mostrar notificação de sincronização completa
    showSyncComplete: function(message) {
        if (window.blazorPWAComponent) {
            window.blazorPWAComponent.invokeMethodAsync('ShowSyncComplete', message);
        }
    },

    // Verificar se está online
    isOnline: function() {
        return navigator.onLine;
    },

    // Verificar se é instalável como PWA
    isInstallable: function() {
        return window.deferredPrompt !== null;
    },

    // Instalar PWA
    installPWA: function() {
        if (window.deferredPrompt) {
            window.deferredPrompt.prompt();
            window.deferredPrompt.userChoice.then((choiceResult) => {
                if (choiceResult.outcome === 'accepted') {
                    console.log('PWA: Usuário aceitou a instalação');
                } else {
                    console.log('PWA: Usuário rejeitou a instalação');
                }
                window.deferredPrompt = null;
            });
        }
    },

    // Registrar para sincronização em background
    registerBackgroundSync: function(tag) {
        if ('serviceWorker' in navigator && 'sync' in window.ServiceWorkerRegistration.prototype) {
            navigator.serviceWorker.ready.then(registration => {
                return registration.sync.register(tag);
            }).then(() => {
                console.log('PWA: Sincronização em background registrada', tag);
            }).catch(error => {
                console.error('PWA: Erro ao registrar sincronização', error);
            });
        }
    },

    // Solicitar permissão para notificações
    requestNotificationPermission: async function() {
        if ('Notification' in window) {
            const permission = await Notification.requestPermission();
            console.log('PWA: Permissão de notificação', permission);
            return permission === 'granted';
        }
        return false;
    },

    // Mostrar notificação local
    showNotification: function(title, options) {
        if ('Notification' in window && Notification.permission === 'granted') {
            new Notification(title, {
                icon: '/icon-192x192.png',
                badge: '/icon-192x192.png',
                ...options
            });
        }
    },

    // Detectar modo de instalação
    detectDisplayMode: function() {
        const isStandalone = window.matchMedia('(display-mode: standalone)').matches;
        const isFullscreen = window.matchMedia('(display-mode: fullscreen)').matches;
        const isMinimalUI = window.matchMedia('(display-mode: minimal-ui)').matches;
        
        if (isStandalone) return 'standalone';
        if (isFullscreen) return 'fullscreen';
        if (isMinimalUI) return 'minimal-ui';
        return 'browser';
    },

    // Armazenar dados offline
    storeOfflineData: function(key, data) {
        try {
            localStorage.setItem(`offline_${key}`, JSON.stringify({
                data: data,
                timestamp: Date.now()
            }));
            console.log('PWA: Dados armazenados offline', key);
            return true;
        } catch (error) {
            console.error('PWA: Erro ao armazenar dados offline', error);
            return false;
        }
    },

    // Recuperar dados offline
    getOfflineData: function(key, maxAge = 86400000) { // 24 horas por padrão
        try {
            const item = localStorage.getItem(`offline_${key}`);
            if (!item) return null;

            const parsed = JSON.parse(item);
            const isExpired = Date.now() - parsed.timestamp > maxAge;
            
            if (isExpired) {
                localStorage.removeItem(`offline_${key}`);
                return null;
            }

            return parsed.data;
        } catch (error) {
            console.error('PWA: Erro ao recuperar dados offline', error);
            return null;
        }
    },

    // Limpar dados offline expirados
    cleanupOfflineData: function() {
        const keys = Object.keys(localStorage);
        const offlineKeys = keys.filter(key => key.startsWith('offline_'));
        
        offlineKeys.forEach(key => {
            try {
                const item = JSON.parse(localStorage.getItem(key));
                const isExpired = Date.now() - item.timestamp > 86400000; // 24 horas
                
                if (isExpired) {
                    localStorage.removeItem(key);
                    console.log('PWA: Dados offline expirados removidos', key);
                }
            } catch (error) {
                // Remove item corrompido
                localStorage.removeItem(key);
            }
        });
    }
};

// Event listeners para PWA
let deferredPrompt = null;

window.addEventListener('beforeinstallprompt', (e) => {
    console.log('PWA: Evento beforeinstallprompt disparado');
    e.preventDefault();
    window.deferredPrompt = e;
});

window.addEventListener('appinstalled', (e) => {
    console.log('PWA: App foi instalado');
    window.deferredPrompt = null;
});

// Detectar mudanças de conectividade
window.addEventListener('online', () => {
    console.log('PWA: Voltou a ficar online');
    window.blazorPWA.registerBackgroundSync('sync-data');
    
    if (window.blazorPWAComponent) {
        window.blazorPWAComponent.invokeMethodAsync('OnConnectivityChanged', true);
    }
});

window.addEventListener('offline', () => {
    console.log('PWA: Ficou offline');
    
    if (window.blazorPWAComponent) {
        window.blazorPWAComponent.invokeMethodAsync('OnConnectivityChanged', false);
    }
});

// Função para configurar referência do componente Blazor
window.blazorPWAComponent = null;
window.blazorPWAComponent = function(componentRef) {
    window.blazorPWAComponent = componentRef;
    console.log('PWA: Componente Blazor registrado');
};

// Registrar Service Worker quando a página carregar
window.addEventListener('load', () => {
    window.blazorPWA.registerServiceWorker();
    window.blazorPWA.cleanupOfflineData();
});

// Tornar disponível globalmente
window.PWA = window.blazorPWA;
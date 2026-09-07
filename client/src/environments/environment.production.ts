export const environment = {
  production: true,
  apiUrl: '/api',
  siteUrl: 'https://layettebaby.com.br',
  analytics: {
    // Blank until real IDs exist — AnalyticsService no-ops without them, same "degrade gracefully"
    // pattern used for WhatsApp/PagBank when their credentials aren't configured yet.
    googleAnalyticsId: '',
    metaPixelId: '',
  },
};

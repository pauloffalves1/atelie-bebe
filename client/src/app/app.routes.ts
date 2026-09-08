import { Routes } from '@angular/router';
import { SITE_NAME } from './core/constants/site';
import { adminGuard } from './core/guards/admin.guard';
import { customerGuard } from './core/guards/customer.guard';

export const routes: Routes = [
  {
    path: 'admin/login',
    title: `Login administrativo — ${SITE_NAME}`,
    loadComponent: () => import('./features/admin/login/admin-login').then((m) => m.AdminLogin),
  },
  {
    path: 'admin',
    loadComponent: () => import('./features/admin/layout/admin-layout').then((m) => m.AdminLayout),
    canActivate: [adminGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'dashboard',
        title: `Dashboard — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/dashboard/admin-dashboard').then((m) => m.AdminDashboard),
      },
      {
        path: 'produtos',
        title: `Produtos — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/products/admin-product-list').then((m) => m.AdminProductList),
      },
      {
        path: 'produtos/novo',
        title: `Novo produto — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/products/admin-product-form').then((m) => m.AdminProductForm),
      },
      {
        path: 'produtos/:id/editar',
        title: `Editar produto — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/products/admin-product-form').then((m) => m.AdminProductForm),
      },
      {
        path: 'encomendas',
        title: `Encomendas — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/orders/admin-order-list').then((m) => m.AdminOrderList),
      },
      {
        path: 'encomendas/:id',
        title: `Detalhe da encomenda — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/orders/admin-order-detail').then((m) => m.AdminOrderDetail),
      },
      {
        path: 'cupons',
        title: `Cupons — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/coupons/admin-coupon-list').then((m) => m.AdminCouponList),
      },
      {
        path: 'mensagens',
        title: `Mensagens de contato — ${SITE_NAME}`,
        loadComponent: () =>
          import('./features/admin/contact-messages/admin-contact-messages').then((m) => m.AdminContactMessages),
      },
      {
        path: 'clientes',
        title: `Clientes — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/customers/admin-customer-list').then((m) => m.AdminCustomerList),
      },
      {
        path: 'clientes/:id/editar',
        title: `Editar cliente — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/customers/admin-customer-form').then((m) => m.AdminCustomerForm),
      },
      {
        path: 'imagens',
        title: `Imagens do site — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/site-images/admin-site-images').then((m) => m.AdminSiteImages),
      },
      {
        path: 'galeria',
        title: `Galeria — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/gallery/admin-gallery').then((m) => m.AdminGallery),
      },
      {
        path: 'newsletter',
        title: `Newsletter — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/newsletter/admin-newsletter').then((m) => m.AdminNewsletter),
      },
      {
        path: 'auditoria',
        title: `Auditoria — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/audit-log/admin-audit-log').then((m) => m.AdminAuditLog),
      },
      {
        path: 'seguranca',
        title: `Segurança — ${SITE_NAME}`,
        loadComponent: () => import('./features/admin/security/admin-security').then((m) => m.AdminSecurity),
      },
    ],
  },
  {
    path: '',
    loadComponent: () => import('./features/public/layout/public-layout').then((m) => m.PublicLayout),
    children: [
      {
        path: '',
        title: `${SITE_NAME} — Fraldas de ombro e boca bordadas`,
        loadComponent: () => import('./features/public/home/home').then((m) => m.Home),
      },
      {
        path: 'loja',
        title: `Loja — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/shop/shop').then((m) => m.Shop),
      },
      {
        path: 'produto/:slug',
        title: `Produto — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/product-detail/product-detail').then((m) => m.ProductDetail),
      },
      {
        path: 'carrinho',
        title: `Carrinho — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/cart/cart-page').then((m) => m.CartPage),
      },
      {
        path: 'checkout',
        title: `Finalizar compra — ${SITE_NAME}`,
        canActivate: [customerGuard],
        loadComponent: () => import('./features/public/checkout/checkout').then((m) => m.Checkout),
      },
      { path: 'encomenda-personalizada', redirectTo: 'contato', pathMatch: 'full' },
      {
        path: 'sobre',
        title: `Sobre o ateliê — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/about/about').then((m) => m.About),
      },
      {
        path: 'galeria',
        title: `Galeria — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/gallery/gallery').then((m) => m.Gallery),
      },
      {
        path: 'contato',
        title: `Contato e encomendas — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/contact/contact').then((m) => m.Contact),
      },
      {
        path: 'termos-de-uso',
        title: `Termos de Uso — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/legal/terms-page').then((m) => m.TermsPage),
      },
      {
        path: 'politica-de-privacidade',
        title: `Política de Privacidade — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/legal/privacy-page').then((m) => m.PrivacyPage),
      },
      {
        path: 'entrar',
        title: `Entrar — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/auth/login-page').then((m) => m.LoginPage),
      },
      {
        path: 'cadastro',
        title: `Criar conta — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/auth/register-page').then((m) => m.RegisterPage),
      },
      {
        path: 'esqueci-senha',
        title: `Esqueci minha senha — ${SITE_NAME}`,
        loadComponent: () =>
          import('./features/public/auth/forgot-password-page').then((m) => m.ForgotPasswordPage),
      },
      {
        path: 'redefinir-senha',
        title: `Redefinir senha — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/auth/reset-password-page').then((m) => m.ResetPasswordPage),
      },
      {
        path: 'verificar-email',
        title: `Verificação de e-mail — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/auth/verify-email-page').then((m) => m.VerifyEmailPage),
      },
      {
        path: 'minha-conta',
        title: `Minha conta — ${SITE_NAME}`,
        canActivate: [customerGuard],
        loadComponent: () => import('./features/public/my-account/my-account').then((m) => m.MyAccount),
      },
      {
        path: 'favoritos',
        title: `Meus favoritos — ${SITE_NAME}`,
        canActivate: [customerGuard],
        loadComponent: () => import('./features/public/wishlist/wishlist-page').then((m) => m.WishlistPage),
      },
      {
        path: 'rastrear-pedido',
        title: `Rastrear pedido — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/track-order/track-order').then((m) => m.TrackOrder),
      },
      {
        path: 'pedido/:id',
        title: `Confirmação de pedido — ${SITE_NAME}`,
        loadComponent: () =>
          import('./features/public/order-confirmation/order-confirmation').then((m) => m.OrderConfirmation),
      },
      {
        path: 'pagamento-simulado/:orderId',
        title: `Pagamento (simulação) — ${SITE_NAME}`,
        loadComponent: () => import('./features/public/fake-payment/fake-payment').then((m) => m.FakePayment),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];

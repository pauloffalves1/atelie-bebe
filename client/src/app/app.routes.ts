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
        path: 'minha-conta',
        title: `Minha conta — ${SITE_NAME}`,
        canActivate: [customerGuard],
        loadComponent: () => import('./features/public/my-account/my-account').then((m) => m.MyAccount),
      },
      {
        path: 'pedido/:id',
        title: `Confirmação de pedido — ${SITE_NAME}`,
        loadComponent: () =>
          import('./features/public/order-confirmation/order-confirmation').then((m) => m.OrderConfirmation),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];

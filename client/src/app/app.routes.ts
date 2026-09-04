import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/admin.guard';
import { customerGuard } from './core/guards/customer.guard';

export const routes: Routes = [
  {
    path: 'admin/login',
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
        loadComponent: () => import('./features/admin/dashboard/admin-dashboard').then((m) => m.AdminDashboard),
      },
      {
        path: 'produtos',
        loadComponent: () => import('./features/admin/products/admin-product-list').then((m) => m.AdminProductList),
      },
      {
        path: 'produtos/novo',
        loadComponent: () => import('./features/admin/products/admin-product-form').then((m) => m.AdminProductForm),
      },
      {
        path: 'produtos/:id/editar',
        loadComponent: () => import('./features/admin/products/admin-product-form').then((m) => m.AdminProductForm),
      },
      {
        path: 'encomendas',
        loadComponent: () => import('./features/admin/orders/admin-order-list').then((m) => m.AdminOrderList),
      },
      {
        path: 'encomendas/:id',
        loadComponent: () => import('./features/admin/orders/admin-order-detail').then((m) => m.AdminOrderDetail),
      },
      {
        path: 'mensagens',
        loadComponent: () =>
          import('./features/admin/contact-messages/admin-contact-messages').then((m) => m.AdminContactMessages),
      },
    ],
  },
  {
    path: '',
    loadComponent: () => import('./features/public/layout/public-layout').then((m) => m.PublicLayout),
    children: [
      { path: '', loadComponent: () => import('./features/public/home/home').then((m) => m.Home) },
      { path: 'loja', loadComponent: () => import('./features/public/shop/shop').then((m) => m.Shop) },
      {
        path: 'produto/:slug',
        loadComponent: () => import('./features/public/product-detail/product-detail').then((m) => m.ProductDetail),
      },
      { path: 'carrinho', loadComponent: () => import('./features/public/cart/cart-page').then((m) => m.CartPage) },
      { path: 'checkout', loadComponent: () => import('./features/public/checkout/checkout').then((m) => m.Checkout) },
      {
        path: 'encomenda-personalizada',
        loadComponent: () => import('./features/public/custom-order/custom-order').then((m) => m.CustomOrder),
      },
      { path: 'sobre', loadComponent: () => import('./features/public/about/about').then((m) => m.About) },
      { path: 'galeria', loadComponent: () => import('./features/public/gallery/gallery').then((m) => m.Gallery) },
      { path: 'contato', loadComponent: () => import('./features/public/contact/contact').then((m) => m.Contact) },
      { path: 'entrar', loadComponent: () => import('./features/public/auth/login-page').then((m) => m.LoginPage) },
      {
        path: 'cadastro',
        loadComponent: () => import('./features/public/auth/register-page').then((m) => m.RegisterPage),
      },
      {
        path: 'minha-conta',
        canActivate: [customerGuard],
        loadComponent: () => import('./features/public/my-account/my-account').then((m) => m.MyAccount),
      },
      {
        path: 'pedido/:id',
        loadComponent: () =>
          import('./features/public/order-confirmation/order-confirmation').then((m) => m.OrderConfirmation),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];

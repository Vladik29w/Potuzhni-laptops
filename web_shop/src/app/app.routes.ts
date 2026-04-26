import { Routes } from '@angular/router';
import { MainComponent } from './main/main.component';
import { LaptopDetails } from './components/laptop-details/details.component';
import { OrderComponent } from './components/order/order.component';
import { RegisterComponent } from './components/register/register.component';
import { LoginComponent } from './components/login/login.component';
import { AdminLaptopComponent } from './components/admin-panel/admin-laptop.component';
import { AdminOrdersComponent } from './components/admin-orders.component/admin-orders.component';
import { adminGuard } from './adminGuard';

export const routes: Routes = [
  {
    path: '',
    component: MainComponent,
    title: 'Main page',
    pathMatch: 'full'
  },
  {
    path: 'details/:id',
    component: LaptopDetails,
    title: 'Laptop Details'
  },
  {
    path: 'order',
    component: OrderComponent,
    title: 'Order'
  },
  {
    path: 'register',
    component: RegisterComponent,
    title: 'Registration'
  },
  {
    path: 'login',
    component: LoginComponent,
    title: 'login'
  },
  {
    path: 'admin/laptop',
    component: AdminLaptopComponent,
    title: 'admin laptops',
    canActivate: [adminGuard]
  },
  {
    path: 'admin/order',
    component: AdminOrdersComponent,
    title: 'admin orders',
    canActivate: [adminGuard]
  },
];

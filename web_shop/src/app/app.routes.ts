import { Routes } from '@angular/router';
import { MainComponent } from './main/main.component';
import { LaptopDetails } from './components/laptop-details/details.component';
import { OrderComponent } from './components/order/order.component';
import { RegisterComponent } from './components/register/register.component';
import { LoginComponent } from './components/login/login.component';
import { AdminPanelComponent } from './components/admin-panel/admin-panel.component';

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
    path: 'admin',
    component: AdminPanelComponent,
    title: 'admin'
  },
];

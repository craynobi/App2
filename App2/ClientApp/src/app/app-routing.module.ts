import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';


import { CounterComponent } from './content/counter/counter.component';
import { FetchDataComponent } from './content/fetch-data/fetch-data.component';
import { FooterComponent } from './content/footer/footer.component';
//import { SlideComponent } from './content/slidehome/slide.component';


const routesConfig: Routes = [
  //patch add defaut
  { path: 'home', loadChildren: () => import('./content/home/home-routing.module').then(m => m.HomeRoutingModule) },
  { path: 'product', component: CounterComponent },
  { path: 'buyproduct', component: FetchDataComponent },
  { path: 'newlist', component: FetchDataComponent },
  { path: 'call', component: FetchDataComponent },
  { path: '**', loadChildren: () => import('./content/home/home-routing.module').then(m => m.HomeRoutingModule)}
]

@NgModule({
  declarations: [
    CounterComponent,
    FetchDataComponent,
    FooterComponent,
   // SlideComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forRoot(routesConfig)
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {}

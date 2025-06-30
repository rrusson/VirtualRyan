import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ResumeChatComponent } from './resume-chat.component';

const routes: Routes = [
  { path: '', component: ResumeChatComponent },
  { path: 'resume-chat', component: ResumeChatComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

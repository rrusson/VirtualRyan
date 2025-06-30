import { HttpClientModule } from '@angular/common/http';
import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app.routing.module';
import { App } from './app';
import { ResumeChatComponent } from './resume-chat.component';
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [App],
  imports: [BrowserModule, HttpClientModule, AppRoutingModule, FormsModule, ResumeChatComponent],
  providers: [
    provideBrowserGlobalErrorListeners()
  ],
  bootstrap: [App]
})
export class AppModule { }

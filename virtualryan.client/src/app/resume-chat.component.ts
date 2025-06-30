import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-resume-chat',
  templateUrl: './resume-chat.component.html',
  styleUrls: ['./resume-chat.component.css'],
  standalone: true, // Mark as standalone
  imports: [FormsModule] // Import FormsModule for ngModel
})
export class ResumeChatComponent {
  public chatQuestion: string = '';
  public chatResponse: string = '';
  public loading: boolean = false;

  constructor(private http: HttpClient) {}

  submitQuestion(): void {
    console.log('submitQuestion called with:', this.chatQuestion);
    if (!this.chatQuestion.trim()) {
      console.log('No question entered.');
      return;
    }
 
    this.loading = true;
    this.chatResponse = '';

    this.http.post(
      'Chat/AskQuestion',
      { question: this.chatQuestion },
      { responseType: 'text' }
    ).subscribe(
      (result) => {
        debugger;
        console.log('HTTP response received:', result);
        this.chatResponse = result;
        this.loading = false;
        console.log('chatResponse set to:', this.chatResponse);
      },
      (error) => {
        console.error('HTTP error:', error);
        this.chatResponse = 'Error getting response:' + error;
        this.loading = false;
      }
    );
  }
}

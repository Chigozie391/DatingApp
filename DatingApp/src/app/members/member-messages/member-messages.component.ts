import { Component, OnInit, Input } from '@angular/core';
import { Message } from 'src/app/_models/message';
import { UserService } from 'src/app/_services/user.service';
import { AuthService } from 'src/app/_services/auth.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import * as _ from 'underscore';
import { tap } from 'rxjs/operators'


@Component({
	selector: 'app-member-messages',
	templateUrl: './member-messages.component.html',
	styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
	@Input() userId: number;
	messages: Message[];
	newMessage: any = {};

	constructor(private userService: UserService, private authService: AuthService,
		private alertiftyService: AlertifyService) { }

	ngOnInit() {
		this.loadMessagesThread();

	}

	loadMessagesThread() {
		const currentUserId = +this.authService.decodedToken.nameid;
		this.userService.getMessageThread(this.authService.decodedToken.nameid, this.userId).pipe(

			tap(messages => {
				_.each(messages, (messages: Message) => {
					if (messages.isRead === false && messages.recipientId === currentUserId) {
						this.userService.markAsRead(currentUserId, messages.id);
					}
				})
			})
		).subscribe((messages: Message[]) => {
			this.messages = messages;
		}, error => {
			this.alertiftyService.error(error);
		})
	}

	sendMessage() {
		this.newMessage.recipientId = this.userId;
		this.userService.sendMessage(this.authService.decodedToken.nameid, this.newMessage).subscribe(message => {
			this.messages.unshift(message);
			this.newMessage.content = '';
		}, error => {
			this.alertiftyService.error(error);
		})
	}

}

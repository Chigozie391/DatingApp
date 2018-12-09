import { Component, OnInit, Input } from '@angular/core';
import { User } from '../../_models/User';
import { AuthService } from '../../_services/auth.service';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';

@Component({
	selector: 'app-member-card',
	templateUrl: './member-card.component.html',
	styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
	// to bring in data from parent component, member-list
	// not array because its for a single user
	@Input() user: User;

	constructor(private authServie: AuthService,
		private userService: UserService, private alertify: AlertifyService) { }

	ngOnInit() {
	}

	sendLike(id: number) {
		this.userService.sendLike(this.authServie.decodedToken.nameid, id).subscribe(data => {
			this.alertify.success('You have liked: ' + this.user.knownAs);
		}, error => {
			this.alertify.error(error);
		})
	}
}

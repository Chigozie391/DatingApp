import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/User';
import { AdminService } from 'src/app/_services/admin.service';
import { BsModalService, BsModalRef } from 'ngx-bootstrap';
import { RoleModalComponent } from '../role-modal/role-modal.component';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.css']
})
export class UserManagementComponent implements OnInit {
  users: User[];
  bsModalRef: BsModalRef;

  constructor(private adminService: AdminService, private modalService: BsModalService) {}

  ngOnInit() {
    this.getUserWithRoles();
  }

  getUserWithRoles() {
    this.adminService.getUserWithRoles().subscribe(
      (users: User[]) => {
        this.users = users;
      },
      error => {
        console.log(error);
      }
    );
  }

  editRoleModal(user: User) {
    const initialState = {
      user,
      roles: this.getRolesArray(user)
    };
    this.bsModalRef = this.modalService.show(RoleModalComponent, { initialState });
    this.bsModalRef.content.updateSelectedRoles.subscribe((newRoles: any[]) => {
      //roles to update are stored in the event emitter ouputted from updateSelectedrole in role-modal
      const rolesToUpdate = {
        roleNames: [...newRoles.filter(el => el.checked === true).map(el => el.name)]
      };

      //api connection
      if (rolesToUpdate) {
        this.adminService.updatedUserRoles(user, rolesToUpdate).subscribe(
          () => {
            user.roles = [...rolesToUpdate.roleNames].toString();
          },
          error => {
            console.log(error);
          }
        );
      }
    });
  }

  private getRolesArray(user) {
    const roles = [];
    const userRoles = user.roles;
    const availableRoles: any[] = [
      { name: 'Admin', value: 'Admin' },
      { name: 'Moderator', value: 'Moderator' },
      { name: 'Member', value: 'Member' },
      { name: 'VIP', value: 'VIP' }
    ];

    for (let i = 0; i < availableRoles.length; i++) {
      let isMatch = false;
      for (let j = 0; j < userRoles.length; j++) {
        if (availableRoles[i].name == userRoles[j]) {
          isMatch = true;
          availableRoles[i].checked = true;
          roles.push(availableRoles[i]);
          break;
        }
      }
      if (!isMatch) {
        availableRoles[i].checked = false;
        roles.push(availableRoles[i]);
      }
    }
    return roles;
  }
}

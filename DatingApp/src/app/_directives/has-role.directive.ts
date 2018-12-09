import { Directive, ViewContainerRef, TemplateRef, OnInit, Input } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];
  isVisible = false;

  constructor(
    private viewContainerRef: ViewContainerRef,
    private templateRef: TemplateRef<any>,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const userRole = this.authService.decodedToken.role as Array<string>;
    if (!userRole) {
      this.viewContainerRef.clear();
    }
    // if user has role needed for to render the element
    if (this.authService.roleMatch(this.appHasRole)) {
      if (!this.isVisible) {
        this.isVisible = true;
        this.viewContainerRef.createEmbeddedView(this.templateRef);
      } else {
        // dont know what this code does
        this.isVisible = false;
        this.viewContainerRef.clear();
      }
    }
  }
}

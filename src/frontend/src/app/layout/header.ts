import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-header',
  templateUrl: './header.html',
})
export class Header {
  fullName = input.required<string>();
  role = input.required<string>();
  menuToggle = output<void>();
  logout = output<void>();
}

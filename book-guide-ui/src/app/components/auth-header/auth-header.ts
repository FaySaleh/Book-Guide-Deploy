import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
@Component({
    selector: 'app-auth-header',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './auth-header.html',
    styleUrls: ['./auth-header.css'],
})
 
export class AuthHeaderComponent{
    @Input() title ='';
    @Input() subtitle ='';
    @Input() logoSrc ='/Logo.png';

}
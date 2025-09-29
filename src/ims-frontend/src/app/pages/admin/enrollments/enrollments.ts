import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CourseService } from '../../../services/course';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-admin-enrollments',
  standalone: true,
  imports: [CommonModule, TableModule, CardModule, ProgressSpinnerModule, ButtonModule, ToastModule],
  templateUrl:'./enrollments.html',
  styleUrls: ['./enrollments.scss']
})
export class AdminEnrollmentsComponent implements OnInit {
  enrollments: any[] = [];
  loading = false;

  constructor(private courseSvc: CourseService) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    this.courseSvc.getAllEnrollments().subscribe((list: any[]) => {
      this.enrollments = list;
      this.loading = false;
    });
  }

  //  Export to Excel
  exportExcel() {
    const worksheet = XLSX.utils.json_to_sheet(
      this.enrollments.map(e => ({
        FullName: e.fullName,
        Username: e.username,
        Email: e.email,
        Course: e.courseTitle,
        EnrolledAt: new Date(e.enrolledAt).toLocaleString()
      }))
    );
    const workbook = { Sheets: { data: worksheet }, SheetNames: ['data'] };
    const excelBuffer = XLSX.write(workbook, { bookType: 'xlsx', type: 'array' });
    const data = new Blob([excelBuffer], { type: 'application/octet-stream' });
    saveAs(data, `enrollments_${new Date().toISOString().slice(0,10)}.xlsx`);
  }

  //  Export to PDF
  exportPDF() {
    const doc = new jsPDF();

    autoTable(doc, {
      head: [['Full Name', 'Username', 'Email', 'Course', 'Enrolled At']],
      body: this.enrollments.map(e => [
        e.fullName,
        e.username,
        e.email,
        e.courseTitle,
        new Date(e.enrolledAt).toLocaleString()
      ]),
      styles: { fontSize: 10 },
      headStyles: { fillColor: [41, 128, 185] }
    });

    doc.save(`enrollments_${new Date().toISOString().slice(0,10)}.pdf`);
  }
}

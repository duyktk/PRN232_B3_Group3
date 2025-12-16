using Microsoft.EntityFrameworkCore;
using Repository.Models;
using Repository.BussinessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class UnitOfWork
    {
        private PRN232_GradingSystem_APIContext _context = new PRN232_GradingSystem_APIContext();
        private GenericRepository<User> _userRepository;
        private GenericRepository<Exam> _examRepository;
        private GenericRepository<Grade> _gradeRepository;
        private GenericRepository<Gradedetail> _gradedetailRepository;
        private GenericRepository<Group> _groupRepository;
        private GenericRepository<GroupStudent> _groupStudentRepository;
        private GenericRepository<Semester> _semesterRepository;
        private GenericRepository<SemesterSubject> _semesterSubjectRepository;
        private GenericRepository<Student> _studentRepository;
        private GenericRepository<Subject> _subjectRepository;
        private GenericRepository<Submission> _submissionRepository;


        public UnitOfWork()
        {

        }

        public GenericRepository<User> UserRepository
        {
            get
            {

                if (_userRepository == null)
                {
                    _userRepository = new GenericRepository<User>(_context);
                }
                return _userRepository;
            }
        }

        public GenericRepository<Exam> ExamRepository
        {
            get
            {

                if (_examRepository == null)
                {
                    _examRepository = new GenericRepository<Exam>(_context);
                }
                return _examRepository;
            }
        }

        public GenericRepository<Grade> GradeRepository
        {
            get
            {

                if (_gradeRepository == null)
                {
                    _gradeRepository = new GenericRepository<Grade>(_context);
                }
                return _gradeRepository;
            }
        }

        public GenericRepository<Gradedetail> GradedetailRepository
        {
            get
            {

                if (_gradedetailRepository == null)
                {
                    _gradedetailRepository = new GenericRepository<Gradedetail>(_context);
                }
                return _gradedetailRepository;
            }
        }

        public GenericRepository<Group> GroupRepository
        {
            get
            {

                if (_groupRepository == null)
                {
                    _groupRepository = new GenericRepository<Group>(_context);
                }
                return _groupRepository;
            }
        }

        public GenericRepository<GroupStudent> GroupStudentRepository
        {
            get
            {

                if (_groupStudentRepository == null)
                {
                    _groupStudentRepository = new GenericRepository<GroupStudent>(_context);
                }
                return _groupStudentRepository;
            }
        }

        public GenericRepository<Semester> SemesterRepository
        {
            get
            {

                if (_semesterRepository == null)
                {
                    _semesterRepository = new GenericRepository<Semester>(_context);
                }
                return _semesterRepository;
            }
        }

        public GenericRepository<SemesterSubject> SemesterSubjectRepository
        {
            get
            {

                if (_semesterSubjectRepository == null)
                {
                    _semesterSubjectRepository = new GenericRepository<SemesterSubject>(_context);
                }
                return _semesterSubjectRepository;
            }
        }

        public GenericRepository<Student> StudentRepository
        {
            get
            {

                if (_studentRepository == null)
                {
                    _studentRepository = new GenericRepository<Student>(_context);
                }
                return _studentRepository;
            }
        }

        public GenericRepository<Subject> SubjectRepository
        {
            get
            {

                if (_subjectRepository == null)
                {
                    _subjectRepository = new GenericRepository<Subject>(_context);
                }
                return _subjectRepository;
            }
        }

        public GenericRepository<Submission> SubmissionRepository
        {
            get
            {

                if (_submissionRepository == null)
                {
                    _submissionRepository = new GenericRepository<Submission>(_context);
                }
                return _submissionRepository;
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<Submission> GetSubmissionByIdAsync(int id, bool includeDetails = true)
         => includeDetails 
            ? await _context.Submissions.Include(s => s.Exam).Include(s => s.Student).Include(s => s.Grades).Where(s => s.Submissionid.Equals(id)).FirstOrDefaultAsync()
            : await _context.Submissions.Where(s => s.Submissionid.Equals(id)).FirstOrDefaultAsync();

        public async Task<User?> Login(string username, string password)
            => await _context.Users.Where(u => u.Username.Equals(username) && password.Equals(password)).FirstOrDefaultAsync();

        public async Task<Exam> GetExamWithFullDataAsync(int examId)
        {
            return await _context.Exams
                .Include(e => e.Submissions)
                    .ThenInclude(s => s.Student)
                        .ThenInclude(st => st.GroupStudents)
                            .ThenInclude(gs => gs.Group)
                .Include(e => e.Submissions)
                    .ThenInclude(s => s.Grades)
                        .ThenInclude(g => g.Gradedetails)
                .Include(e => e.Submissions)
                    .ThenInclude(s => s.Grades)
                        .ThenInclude(g => g.MarkerNavigation)
                .FirstOrDefaultAsync(e => e.Examid == examId);
        }
    }
}

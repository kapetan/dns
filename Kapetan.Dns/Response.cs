using System;
using System.Collections.Generic;
using System.Linq;
using Kapetan.Dns.Interface;
using Kapetan.Dns.Model;

namespace Kapetan.Dns
{
    public class Response : IResponse
    {
        private static readonly Random RANDOM = new Random();

        private Header header;
        private IList<Question> questions;
        private IList<IResourceRecord> answers;
        private IList<IResourceRecord> authority;
        private IList<IResourceRecord> additional;

        public static Response FromRequest(IRequest request)
        {
            var response = new Response();
            response.Id = request.Id;

            foreach (var question in request.Questions)
            {
                response.Questions.Add(question);
            }

            return response;
        }

        public static Response FromArray(byte[] message)
        {
            var header = Header.FromArray(message);
            var offset = header.Size;

            if (!header.Response || header.QuestionCount == 0)
            {
                throw new ArgumentException("Invalid response message");
            }

            if (header.Truncated)
            {
                return new Response(header,
                    Question.GetAllFromArray(message, offset, header.QuestionCount),
                    new List<IResourceRecord>(),
                    new List<IResourceRecord>(),
                    new List<IResourceRecord>());
            }

            return new Response(header,
                Question.GetAllFromArray(message, offset, header.QuestionCount, out offset),
                ResourceRecordFactory.GetAllFromArray(message, offset, header.AnswerRecordCount, out offset),
                ResourceRecordFactory.GetAllFromArray(message, offset, header.AuthorityRecordCount, out offset),
                ResourceRecordFactory.GetAllFromArray(message, offset, header.AdditionalRecordCount, out offset));
        }

        public Response(Header header, IList<Question> questions, IList<IResourceRecord> answers,
                IList<IResourceRecord> authority, IList<IResourceRecord> additional)
        {
            this.header = header;
            this.questions = questions;
            this.answers = answers;
            this.authority = authority;
            this.additional = additional;
        }

        public Response()
        {
            this.header = new Header();
            this.questions = new List<Question>();
            this.answers = new List<IResourceRecord>();
            this.authority = new List<IResourceRecord>();
            this.additional = new List<IResourceRecord>();

            this.header.Response = true;
            this.header.Id = RANDOM.Next(UInt16.MaxValue);
        }

        public Response(IResponse response)
        {
            this.header = new Header();
            this.questions = new List<Question>(response.Questions);
            this.answers = new List<IResourceRecord>(response.AnswerRecords);
            this.authority = new List<IResourceRecord>(response.AuthorityRecords);
            this.additional = new List<IResourceRecord>(response.AdditionalRecords);

            this.header.Response = true;

            this.Id = response.Id;
            this.RecursionAvailable = response.RecursionAvailable;
            this.AuthorativeServer = response.AuthorativeServer;
            this.OperationCode = response.OperationCode;
            this.ResponseCode = response.ResponseCode;
        }

        public IList<Question> Questions
        {
            get { return this.questions; }
        }

        public IList<IResourceRecord> AnswerRecords
        {
            get { return this.answers; }
        }

        public IList<IResourceRecord> AuthorityRecords
        {
            get { return this.authority; }
        }

        public IList<IResourceRecord> AdditionalRecords
        {
            get { return this.additional; }
        }

        public int Id
        {
            get { return this.header.Id; }
            set { this.header.Id = value; }
        }

        public bool RecursionAvailable
        {
            get { return this.header.RecursionAvailable; }
            set { this.header.RecursionAvailable = value; }
        }

        public bool AuthorativeServer
        {
            get { return this.header.AuthorativeServer; }
            set { this.header.AuthorativeServer = value; }
        }

        public bool Truncated
        {
            get { return this.header.Truncated; }
            set { this.header.Truncated = value; }
        }

        public OperationCode OperationCode
        {
            get { return this.header.OperationCode; }
            set { this.header.OperationCode = value; }
        }

        public ResponseCode ResponseCode
        {
            get { return this.header.ResponseCode; }
            set { this.header.ResponseCode = value; }
        }

        public int Size
        {
            get
            {
                return this.header.Size +
                    this.questions.Sum(q => q.Size) +
                    this.answers.Sum(a => a.Size) +
                    this.authority.Sum(a => a.Size) +
                    this.additional.Sum(a => a.Size);
            }
        }

        public byte[] ToArray()
        {
            this.UpdateHeader();
            var result = new Marshalling.ByteStream(Size);

            result
                .Append(header.ToArray())
                .Append(questions.Select(q => q.ToArray()))
                .Append(answers.Select(a => a.ToArray()))
                .Append(authority.Select(a => a.ToArray()))
                .Append(additional.Select(a => a.ToArray()));

            return result.ToArray();
        }

        public override string ToString()
        {
            this.UpdateHeader();

            return Marshalling.Object.New(this)
                .Add("Header", header)
                .Add("Questions", "AnswerRecords", "AuthorityRecords", "AdditionalRecords")
                .ToString();
        }

        private void UpdateHeader()
        {
            this.header.QuestionCount = questions.Count;
            this.header.AnswerRecordCount = answers.Count;
            this.header.AuthorityRecordCount = authority.Count;
            this.header.AdditionalRecordCount = additional.Count;
        }
    }
}
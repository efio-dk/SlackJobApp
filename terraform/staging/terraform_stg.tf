# Region
provider "aws" {
  region = "eu-west-1"
}

# Data resources (SQS)
data "aws_sqs_queue" "stg-processed-job-post-queue" {
  name = "stg_ProcessedJobPosts"
}

# Lambda
resource "aws_lambda_function" "stg-SlackJobPoster-lambda" {
  function_name    = "stg-SlackJobPoster"
  handler          = "SlackJobPoster::SlackJobPoster.Function::FunctionHandler"
  runtime          = "dotnetcore2.1"
  role             = "arn:aws:iam::833191605868:role/SlackJobPosterRole"
  filename         = "../../src/SlackJobPoster/bin/Debug/netcoreapp2.1/SlackJobPoster.zip"
  source_code_hash = filebase64sha256("../../src/SlackJobPoster/bin/Debug/netcoreapp2.1/SlackJobPoster.zip")
  timeout          = 10

  tags = {
    Name        = "stg-SlackJobPoster"
    Environment = "staging"
  }
}

resource "aws_lambda_event_source_mapping" "stg-incoming-sqs" {
  event_source_arn = data.aws_sqs_queue.stg-processed-job-post-queue.arn
  function_name    = aws_lambda_function.stg-SlackJobPoster-lambda.arn
}
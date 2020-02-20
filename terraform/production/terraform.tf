terraform {
 backend "s3" {
 encrypt = true
 bucket = "prod-terraform-slack-job-poster-storage-s3"
 region = "eu-west-1"
 key = "terraform.tfstate"
 }
}